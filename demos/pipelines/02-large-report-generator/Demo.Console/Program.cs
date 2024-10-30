using Demo.Console.Dtos.Inputs;
using Demo.Console.Generators;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

SalesReportInputDto input = new(1, DateTime.Now.AddDays(-365), DateTime.Now);

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

var configuration = builder.Build();

string connectionString = configuration["ConnectionStrings:SalesDB"] ?? 
    throw new InvalidOperationException("Connection string has not been found");

Stopwatch stopwatch = new();

stopwatch.Start();

string outputFilePath = await new SalesPipeReportGenerator(connectionString).GenerateAsync(input);

stopwatch.Stop();

Console.WriteLine($"Finished generation in {stopwatch.ElapsedMilliseconds} ms. File available at {outputFilePath}");