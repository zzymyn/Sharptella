using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Sharptari.Lib;

namespace Sharptari.CpuTest;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var testFiles = ResolveFilePaths(args);

        var testResults = new Dictionary<string, bool>();

        await foreach (var (name, tests) in LoadAllTests(testFiles))
        {
            Console.Write($"Running {name}: ");

            int totalCount = 0;
            int successCount = 0;

            Parallel.ForEach(tests,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                test =>
                {
                    bool success = false;

                    try
                    {
                        success = RunTest(test);
                    }
                    catch (Exception)
                    {
                        success = false;
                    }

                    Interlocked.Increment(ref totalCount);
                    if (success)
                    {
                        Interlocked.Increment(ref successCount);
                    }
                });

            if (successCount != totalCount)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                testResults.Add(name, false);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                testResults.Add(name, true);
            }
            Console.WriteLine($"{successCount}/{totalCount} tests passed.");
            Console.ResetColor();
        }

        sw.Stop();
        Console.WriteLine($"Tests ran in {sw.Elapsed.TotalSeconds:F2} seconds.");
        Console.WriteLine();

        var failedTests = testResults.Where(kv => !kv.Value).Select(kv => kv.Key).ToList();
        if (failedTests.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Failed tests:");
            foreach (var testName in failedTests)
            {
                Console.Write($" {testName}");
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All tests passed successfully!");
        }
        Console.ResetColor();
    }

    private static bool RunTest(TestData test)
    {
        var initialResigers = new Mos6502Registers
        {
            A = test.Initial!.A,
            X = test.Initial.X,
            Y = test.Initial.Y,
            S = test.Initial.S,
            PC = test.Initial.PC,
            TestP = test.Initial.P,
        };

        var expectedRegisters = new Mos6502Registers
        {
            A = test.Final!.A,
            X = test.Final.X,
            Y = test.Final.Y,
            S = test.Final.S,
            PC = test.Final.PC,
            TestP = test.Final.P,
        };

        var ram = new Mos6502TestRam();
        foreach (var entry in test.Initial.Ram!)
        {
            ram.WriteDirect(entry.Address, entry.Value);
        }

        var bus = new Mos6502Bus();
        bus.Items.Add(ram);

        var cpu = new Mos6502Cpu(bus, initialResigers);

        // limit to 10 steps because JAM instructions can cause infinite loops
        // and if the test is correct it should reach the next opcode within 10 steps at most
        var stepCount = 0;
        do
        {
            cpu.Step();
            ++stepCount;
        } while (!cpu.IsAtOpCodeStart && stepCount <= 10);

        var finalRegisters = cpu.Registers;

        if (finalRegisters.A != expectedRegisters.A ||
            finalRegisters.X != expectedRegisters.X ||
            finalRegisters.Y != expectedRegisters.Y ||
            finalRegisters.S != expectedRegisters.S ||
            finalRegisters.PC != expectedRegisters.PC ||
            finalRegisters.TestP != expectedRegisters.TestP)
        {
            return false;
        }

        foreach (var tuple in test.Final.Ram!)
        {
            var expectedValue = tuple.Value;
            var actualValue = ram.ReadDirect(tuple.Address);
            if (actualValue != expectedValue)
            {
                return false;
            }
        }

        var log = ram.Log;

        if (log.Count != test.Cycles!.Count)
        {
            return false; // Number of cycles does not match
        }

        for (int i = 0; i < test.Cycles!.Count; i++)
        {
            var expectedCycle = test.Cycles[i]!;
            var actualCycle = log[i];

            if (expectedCycle.Type == CycleType.Read)
            {
                if (!actualCycle.IsRead || actualCycle.Address != expectedCycle.Address || actualCycle.Value != expectedCycle.Value)
                {
                    return false;
                }
            }
            else
            {
                if (actualCycle.IsRead || actualCycle.Address != expectedCycle.Address || actualCycle.Value != expectedCycle.Value)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static List<string> ResolveFilePaths(string[] args)
    {
        var resolvedFiles = new List<string>();

        // Fallback: If no arguments are passed, load everything from the default folder
        if (args.Length == 0)
        {
            resolvedFiles.AddRange(Directory.EnumerateFiles(Environment.CurrentDirectory, "*.json"));
            return resolvedFiles;
        }

        foreach (var arg in args)
        {
            // 1. Check if the argument contains a wildcard (* or ?)
            if (arg.Contains('*') || arg.Contains('?'))
            {
                try
                {
                    var directory = Path.GetDirectoryName(arg);
                    var pattern = Path.GetFileName(arg);

                    // If the path was just "*.json", look in the current working directory
                    if (string.IsNullOrEmpty(directory))
                    {
                        directory = Environment.CurrentDirectory;
                    }

                    if (Directory.Exists(directory))
                    {
                        resolvedFiles.AddRange(Directory.EnumerateFiles(directory, pattern));
                    }
                    else
                    {
                        Console.WriteLine($"Directory not found for wildcard: {directory}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error expanding wildcard path '{arg}': {ex.Message}");
                }
            }
            else if (Directory.Exists(arg))
            {
                // 2. If the argument is an existing directory, search for all *.json inside it
                resolvedFiles.AddRange(Directory.EnumerateFiles(arg, "*.json"));
            }
            else if (File.Exists(arg))
            {
                // 3. If it is an existing file, add it directly
                resolvedFiles.Add(arg);
            }
            else
            {
                Console.WriteLine($"File or path not found: {arg}");
            }
        }

        // De-duplicate in case of overlapping paths
        return resolvedFiles.Distinct().ToList();
    }

    private static IAsyncEnumerable<(string, List<TestData>)> LoadAllTests(List<string> testFiles)
    {
        var channel = Channel.CreateUnbounded<(string, List<TestData>)>(new UnboundedChannelOptions()
        {
            SingleWriter = false,
            SingleReader = true,
        });

        _ = Task.Run(async () =>
        {
            await Parallel.ForEachAsync(testFiles,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                async (testFile, ct) =>
                {
                    var tests = await LoadTests(testFile);
                    await channel.Writer.WriteAsync((Path.GetFileNameWithoutExtension(testFile), tests), ct);
                });

            channel.Writer.Complete();
        });

        return channel.Reader.ReadAllAsync();
    }

    private static async Task<List<TestData>> LoadTests(string filePath)
    {
        try
        {
            var fileData = await File.ReadAllBytesAsync(filePath);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = FastCpuTestParser.Parse(fileData);

            if (result == null)
            {
                return [];
            }
            return result;
        }
        catch (Exception)
        {
            return [];
        }
    }
}