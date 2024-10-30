using Serilog;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace Demo.Console.Generators.Pipeline.Stages;

public class SalesCsvGenerationPipeStage
{
    public async Task ExecuteAsync(string outputFilePath, PipeReader reader)
    {
        try
        {
            using Stream outputStream = File.OpenWrite(outputFilePath);

            await outputStream.WriteAsync(Encoding.UTF8.GetBytes("CompanyId;Description;GrossAmount;TaxAmount;SalesDate\n"));

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
            Log.Error(ex.Message);
        }
        finally
        {
            await reader.CompleteAsync();
        }
    }
}