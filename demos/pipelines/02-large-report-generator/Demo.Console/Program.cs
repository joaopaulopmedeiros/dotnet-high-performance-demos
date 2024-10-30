using Demo.Console.Dtos.Inputs;
using Demo.Console.Generators;
using System.Diagnostics;

SalesReportInputDto input = new(Guid.NewGuid().ToString(), DateTime.Now.AddDays(-5), DateTime.Now);

Stopwatch stopwatch = new();

stopwatch.Start();

string outputFilePath = await new SalesPipeReportGenerator().GenerateAsync(input);

stopwatch.Stop();

Console.WriteLine($"Finished generation in {stopwatch.ElapsedMilliseconds} ms. File available at {outputFilePath}");