using Demo.Console.Dtos.Inputs;

namespace Demo.Console.Generators;

public interface ISalesReportGenerator
{
    public Task<string> GenerateAsync(SalesReportInputDto input);
}