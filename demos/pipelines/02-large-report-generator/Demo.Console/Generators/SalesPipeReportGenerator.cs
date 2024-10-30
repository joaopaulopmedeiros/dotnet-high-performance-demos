using Dapper;
using Demo.Console.Dtos.Inputs;
using Npgsql;
using System.Buffers;
using System.Data;
using System.IO.Pipelines;
using System.Text;

namespace Demo.Console.Generators;

public class SalesPipeReportGenerator(string connectionString) : ISalesReportGenerator
{
    private readonly string _connectionString = connectionString;
    private readonly int _minimumBufferSize = 1024;

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
        try
        {
            await using NpgsqlConnection connection = new(_connectionString);

            string query = @"SELECT company_id AS CompanyId, 
                                description AS Description, 
                                gross_amount AS GrossAmount, 
                                tax_amount AS TaxAmount, 
                                sales_date AS SalesDate 
                         FROM sales 
                         WHERE company_id = @CompanyId 
                           AND sales_date >= @StartDate 
                           AND sales_date <= @EndDate";

            var parameters = new
            {
                input.CompanyId,
                input.StartDate,
                input.EndDate,
            };

            await connection.OpenAsync();

            using var reader = await connection.ExecuteReaderAsync(query, parameters);

            while (await reader.ReadAsync())
            {
                string reportRow = $"{reader["CompanyId"]},{reader["Description"]},{reader.GetDecimal("GrossAmount"):0.00},{reader.GetDecimal("TaxAmount"):0.00},{reader.GetDateTime("SalesDate"):yyyy-MM-dd}\n";

                Memory<byte> memory = writer.GetMemory(_minimumBufferSize);

                int bytesWritten = Encoding.UTF8.GetBytes(reportRow, memory.Span);

                writer.Advance(bytesWritten);

                FlushResult flushResult = await writer.FlushAsync();

                if (flushResult.IsCompleted) break;
            }
        } 
        catch (Exception ex) 
        {
            System.Console.WriteLine(ex.Message);    
        }
        finally
        {
            await writer.CompleteAsync();
        }
    }

    private async Task ReadPipeToGenerateReportAsync(string outputFilePath, PipeReader reader)
    {
        try
        {
            using Stream outputStream = File.OpenWrite(outputFilePath);

            while (true)
            {
                ReadResult result = await reader.ReadAsync();

                ReadOnlySequence<byte> buffer = result.Buffer;

                foreach (ReadOnlyMemory<byte> segment in buffer)
                {
                    await outputStream.WriteAsync(segment);
                }

                reader.AdvanceTo(buffer.End);

                if (result.IsCompleted) break;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }
        finally
        {
            await reader.CompleteAsync();
        }
    }
}
