using Demo.Console.Dtos.Inputs;
using Demo.Console.Factories;
using System.IO.Pipelines;

namespace Demo.Console.Generators.Pipeline;

public class SalesPipeReportGenerator(
    SalesSearchPipeStage searchStage,
    SalesCsvGenerationPipeStage generationStage
    ) : ISalesReportGenerator
{
    private readonly SalesSearchPipeStage _searchStage = searchStage;
    private readonly SalesCsvGenerationPipeStage _generationStage = generationStage;

    public async Task<string> GenerateAsync(SalesReportInputDto input)
    {
        string outputFilePath = SalesReportKeyFactory.Create(
            input.CompanyId,
            input.StartDate,
            input.EndDate);

        Pipe pipe = new();

        await Task.WhenAll(
            _searchStage.ExecuteAsync(input, pipe.Writer),
            _generationStage.ExecuteAsync(outputFilePath, pipe.Reader)
        );

        return outputFilePath;
    }
}