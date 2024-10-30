using Demo.Console.Dtos.Inputs;

namespace Demo.Console.Generators;

public class SalesPipeReportGenerator : ISalesReportGenerator
{
    public async Task<string> GenerateAsync(SalesReportInputDto input)
    {
        await Task.Delay(1000);
        return SalesReportKeyFactory.Create(input.CompanyId, input.StartDate, input.EndDate);
    }
}
