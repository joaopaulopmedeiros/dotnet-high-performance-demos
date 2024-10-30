using Dapper;
using Demo.Console.Dtos.Inputs;
using Npgsql;
using Serilog;
using System.Data;
using System.IO.Pipelines;
using System.Text;

namespace Demo.Console.Generators.Pipeline.Stages;

public class SalesSearchPipeStage(string connectionString)
{
    private readonly string _connectionString = connectionString;
    private const int BatchSize = 20_000;

    public async Task ExecuteAsync(SalesReportInputDto input, PipeWriter writer)
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

            StringBuilder sb = new(BatchSize);

            while (await reader.ReadAsync())
            {
                sb.AppendLine($"{reader["CompanyId"]};{reader["Description"]};{reader.GetDecimal("GrossAmount"):0.00};{reader.GetDecimal("TaxAmount"):0.00};{reader.GetDateTime("SalesDate"):yyyy-MM-dd}");

                if (sb.Length >= BatchSize)
                {
                    Memory<byte> memory = writer.GetMemory(sb.Length);

                    int bytesWritten = Encoding.UTF8.GetBytes(sb.ToString(), memory.Span);

                    writer.Advance(bytesWritten);

                    sb.Clear();

                    FlushResult flushResult = await writer.FlushAsync();

                    if (flushResult.IsCompleted) break; 
                }
            }

            if (sb.Length > 0)
            {
                Memory<byte> memory = writer.GetMemory(sb.Length);

                int bytesWritten = Encoding.UTF8.GetBytes(sb.ToString(), memory.Span);

                writer.Advance(bytesWritten);

                await writer.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
        finally
        {
            await writer.CompleteAsync();
        }
    }
}
