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
    private static readonly HashSet<string> UnstableOpcodes =
    [
        // Highly Unstable Group
        "8b", // ANE / XAA (Immediate)
        "ab", // LXA / ATX (Immediate)

        // "Sometimes Unstable" Group
        "93", // SHA / AHX (Indirect, Y)
        "9f", // SHA / AHX (Absolute, Y)
        "9e", // SHX / SXA (Absolute, Y)
        "9c", // SHY / SYA (Absolute, X)
        "9b", // TAS / SHS (Absolute, Y)
    ];

    private static async Task Main(string[] args)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var testFiles = ResolveFilePaths(args);

        var failedTests = new HashSet<string>();

        await foreach (var (name, tests) in LoadAllTests(testFiles))
        {
            Console.Write($"Running {name}: ");

            int totalCount = 0;
            int successCount = 0;

            Parallel.ForEach(tests,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                test =>
                {
                    //if ((test.Initial.P & Mos6502Registers.FlagDecimalMask) != 0)
                    //{
                    //    return;
                    //}

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

            if (UnstableOpcodes.Contains(name))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if (successCount != totalCount)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                failedTests.Add(name);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine($"{successCount}/{totalCount} tests passed.");
            Console.ResetColor();
        }

        sw.Stop();
        Console.WriteLine($"Tests ran in {sw.Elapsed.TotalSeconds:F2} seconds.");
        Console.WriteLine();

        if (failedTests.Count > 0)
        {
            Console.Write("Failed tests:");
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var testName in failedTests.Order())
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
        Console.WriteLine();
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

        var bus = new Mos6502TestBus();
        foreach (var entry in test.Initial.Ram!)
        {
            bus.WriteDirect(entry.Address, entry.Value);
        }

        var cpu = new Mos6502Cpu<Mos6502TestBus>(bus, initialResigers);

        // limit to 10 steps because JAM instructions can cause infinite loops
        // and if the test is correct it should reach the next opcode within 10 steps at most
        var stepCount = 0;
        do
        {
            cpu.Step();
            ++stepCount;
        } while (!cpu.IsAtOpCodeStart && stepCount <= 10);

        var finalRegisters = cpu.Registers;

        if (finalRegisters.A != expectedRegisters.A)
        {
            return false;
        }
        if (finalRegisters.X != expectedRegisters.X)
        {
            return false;
        }
        if (finalRegisters.Y != expectedRegisters.Y)
        {
            return false;
        }
        if (finalRegisters.S != expectedRegisters.S)
        {
            return false;
        }
        if (finalRegisters.PC != expectedRegisters.PC)
        {
            return false;
        }
        if (finalRegisters.PNegative != expectedRegisters.PNegative)
        {
            return false;
        }
        if (finalRegisters.POverflow != expectedRegisters.POverflow)
        {
            return false;
        }
        if (finalRegisters.PDecimal != expectedRegisters.PDecimal)
        {
            return false;
        }
        if (finalRegisters.PInterruptDisable != expectedRegisters.PInterruptDisable)
        {
            return false;
        }
        if (finalRegisters.PZero != expectedRegisters.PZero)
        {
            return false;
        }
        if (finalRegisters.PCarry != expectedRegisters.PCarry)
        {
            return false;
        }
        if (finalRegisters.TestP != expectedRegisters.TestP)
        {
            return false;
        }

        foreach (var tuple in test.Final.Ram!)
        {
            var expectedValue = tuple.Value;
            var actualValue = bus.ReadDirect(tuple.Address);
            if (actualValue != expectedValue)
            {
                return false;
            }
        }

        var log = bus.Log;

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