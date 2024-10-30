namespace Demo.Console.Generators;

public static class SalesReportKeyFactory
{
    public static string Create(int companyId, DateTime startDate, DateTime endDate)
        => $"C://tmp/sales//report-{companyId}-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.csv";
}
