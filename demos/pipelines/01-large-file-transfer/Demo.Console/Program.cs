using Demo.Console.Processors;
using System.Diagnostics;

const string inputFilePath = "C://tmp/reports/pipelines/input/custom_1988_2020.csv";
const string outputFilePath = "C://tmp/reports/pipelines/output/custom_1988_2020.csv";

Stopwatch stopwatch = new();

stopwatch.Start();

Console.WriteLine("Start transfer...");

await new FileTransfer().TransferAsync(inputFilePath, outputFilePath);

stopwatch.Stop();

Console.WriteLine($"Finish transfer in {stopwatch.ElapsedMilliseconds} ms");