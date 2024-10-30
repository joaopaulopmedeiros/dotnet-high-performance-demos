using Demo.Console.Dtos.Inputs;
using Demo.Console.Generators;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Diagnostics;

SalesReportInputDto input = new(1, DateTime.Now.AddYears(-2), DateTime.Now);

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

var configuration = builder.Build();

string connectionString = configuration["ConnectionStrings:SalesDB"] ?? 
    throw new InvalidOperationException("Connection string has not been found");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Stopwatch stopwatch = new();

stopwatch.Start();

Log.Information($"Report generation started. Working on it...");

string outputFilePath = await new SalesPipeReportGenerator(connectionString).GenerateAsync(input);

stopwatch.Stop();

Log.Information($"Report generation finished in {stopwatch.ElapsedMilliseconds} ms. File available at {outputFilePath}");