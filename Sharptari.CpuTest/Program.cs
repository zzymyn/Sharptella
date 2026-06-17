using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Sharptari.Lib;

namespace Sharptari.CpuTest;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var testFiles = ResolveFilePaths(args);

        await foreach (var (name, tests) in LoadAllTests(testFiles))
        {
            Console.Write($"Running {name}: ");

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

                    if (success)
                    {
                        Interlocked.Increment(ref successCount);
                    }
                });

            if (successCount != tests.Length)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine($"{successCount}/{tests.Length} tests passed.");
            Console.ResetColor();
        }
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
        foreach (var tuple in test.Initial.Ram!)
        {
            ram.WriteDirect((ushort)tuple![0], (byte)tuple[1]);
        }

        var bus = new Mos6502Bus();
        bus.Items.Add(ram);

        var cpu = new Mos6502Cpu(bus, initialResigers);

        do
        {
            cpu.Step();
        } while (!cpu.IsAtOpCodeStart);

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
            var expectedValue = (byte)tuple![1];
            var actualValue = ram.ReadDirect((ushort)tuple[0]);
            if (actualValue != expectedValue)
            {
                return false;
            }
        }

        var log = ram.Log;

        if (log.Count != test.Cycles!.Length)
        {
            return false; // Number of cycles does not match
        }

        for (int i = 0; i < test.Cycles!.Length; i++)
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

    private static IAsyncEnumerable<(string, TestData[])> LoadAllTests(List<string> testFiles)
    {
        var channel = Channel.CreateBounded<(string, TestData[])>(new BoundedChannelOptions(4)
        {
            SingleWriter = true,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait // Wait/Pause the writer if the buffer is full
        });

        _ = Task.Run(async () =>
        {
            foreach (var testFile in testFiles)
            {
                var tests = await LoadTests(testFile);
                await channel.Writer.WriteAsync((Path.GetFileNameWithoutExtension(testFile), tests));
            }

            channel.Writer.Complete();
        });

        return channel.Reader.ReadAllAsync();
    }

    private static async Task<TestData[]> LoadTests(string filePath)
    {
        try
        {
            var fileData = await File.ReadAllBytesAsync(filePath);

            var result = JsonSerializer.Deserialize<TestData[]>(fileData);

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