using System;
using System.IO;
using System.Linq;

internal class Program
{
    private static void Main(string[] args)
    {
        var testDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "Ext", "SingleStepTests", "65x02", "6502"));

        var testFiles = Directory.EnumerateFiles(testDir, "*.json").ToList();

        foreach (var testFile in testFiles)
        {
            var testData = System.Text.Json.JsonSerializer.Deserialize<Sharptari.CpuTest.TestData>(File.ReadAllText(testFile));
            if (testData is null)
            {
                Console.WriteLine($"Failed to read test data from {testFile}");
                continue;
            }
            Console.WriteLine($"Read test data from {testFile}: {testData.Name}");
        }
    }
}