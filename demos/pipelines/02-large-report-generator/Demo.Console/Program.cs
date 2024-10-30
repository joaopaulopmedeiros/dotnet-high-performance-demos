using Demo.Console.Dtos.Inputs;
using Demo.Console.Generators.Pipeline;
using Demo.Console.Generators.Pipeline.Stages;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Diagnostics;

SalesReportInputDto input = new(1, DateTime.Now.AddYears(-2), DateTime.Now);

IConfigurationBuilder builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

IConfigurationRoot configuration = builder.Build();

string connectionString = configuration["ConnectionStrings:SalesDB"] ??
    throw new InvalidOperationException("Connection string has not been found");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Stopwatch stopwatch = new();

stopwatch.Start();

Log.Information($"Report generation started. Working on it...");

SalesSearchPipeStage searchStage = new(connectionString);
SalesCsvGenerationPipeStage generationStage = new();

string outputFilePath = await new SalesPipeReportGenerator(searchStage, generationStage).GenerateAsync(input);

stopwatch.Stop();

Log.Information($"Report generation finished in {stopwatch.ElapsedMilliseconds} ms. File available at {outputFilePath}");