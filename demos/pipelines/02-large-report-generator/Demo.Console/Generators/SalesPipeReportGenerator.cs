using Dapper;
using Demo.Console.Dtos.Inputs;
using Npgsql;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace Demo.Console.Generators;

public class SalesPipeReportGenerator(string connectionString) : ISalesReportGenerator
{
    private readonly string _connectionString = connectionString;
    private readonly int _minimumBufferSize = 1024 * 1024;

    public async Task<string> GenerateAsync(SalesReportInputDto input)
    {
        string outputFilePath = SalesReportKeyFactory.Create(
            input.CompanyId, 
            input.StartDate, 
            input.EndDate);

        Pipe pipe = new();

        Task searchRecords = FillPipeWithSearchRecordsAsync(input, pipe.Writer);
        Task generateReport = ReadPipeToGenerateReportAsync(outputFilePath, pipe.Reader);

        await Task.WhenAll(searchRecords, generateReport);

        return outputFilePath;
    }

    private async Task FillPipeWithSearchRecordsAsync(SalesReportInputDto input, PipeWriter writer)
    {
        await using NpgsqlConnection connection = new(_connectionString);

        string query = "SELECT * FROM sales WHERE company_id = @CompanyId AND created_at >= @StartDate AND created_at <= @EndDate";

        dynamic parameters = new
        {
            input.CompanyId,
            input.StartDate,
            input.EndDate,
        };

        CommandDefinition command = new(query, parameters: parameters, commandTimeout: 60);

        await foreach (var record in connection.QueryUnbufferedAsync(query))
        {
            string reportRow = $"{record.CompanyId},{record.Description},{record.GrossAmount:0.00},{record.TaxAmount:0.00},{record.SalesDate:yyyy-MM-dd}\n";

            Memory<byte> memory = writer.GetMemory(_minimumBufferSize);

            int bytesWritten = Encoding.UTF8.GetBytes(reportRow, memory.Span);

            writer.Advance(bytesWritten);

            FlushResult flushResult = await writer.FlushAsync();

            if (flushResult.IsCompleted) break;
        }

        await writer.CompleteAsync();
    }

    private async Task ReadPipeToGenerateReportAsync(string outputFilePath, PipeReader reader)
    {
        using Stream outputStream = File.OpenWrite(outputFilePath);

        while (true)
        {
            ReadResult result = await reader.ReadAsync();

            ReadOnlySequence<byte> buffer = result.Buffer;

            foreach(ReadOnlyMemory<byte> segment in buffer)
            {
                await outputStream.WriteAsync(segment);
            }

            reader.AdvanceTo(buffer.End);

            if (result.IsCompleted) break;
        }

        await reader.CompleteAsync();
    }
}
