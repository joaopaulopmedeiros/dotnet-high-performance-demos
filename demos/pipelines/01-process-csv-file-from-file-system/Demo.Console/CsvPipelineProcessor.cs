using System.Buffers;
using System.IO.Pipelines;

namespace Demo.Console;

public class CsvPipelineProcessor : ICsvProcessor
{
    public async Task ProcessAsync(string inputFilePath, string outputFilePath)
    {
        using Stream inputStream = File.OpenRead(inputFilePath);
        using Stream outputStream = File.OpenWrite(outputFilePath);

        var pipe = new Pipe();

        var fillPipeTask = FillPipeAsync(inputStream, pipe.Writer);
        var readPipeTask = ReadPipeAsync(outputStream, pipe.Reader);

        await Task.WhenAll(fillPipeTask, readPipeTask);
    }

    private static async Task FillPipeAsync(Stream inputStream, PipeWriter writer)
    {
        const int bufferSize = 1024;
        
        while(true)
        {
            Memory<byte> memory = writer.GetMemory(bufferSize);
            
            int bytesRead = await inputStream.ReadAsync(memory);
            
            if (bytesRead == 0) break;

            writer.Advance(bytesRead);

            FlushResult result = await writer.FlushAsync();

            if (result.IsCompleted) break;
        }

        await writer.CompleteAsync();
    }

    private static async Task ReadPipeAsync(Stream outputStream, PipeReader reader)
    {
        while (true)
        {
            ReadResult result = await reader.ReadAsync();

            ReadOnlySequence<byte> buffer = result.Buffer;

            if (buffer.Length > 0)
            {
                foreach (ReadOnlyMemory<byte> segment in buffer)
                {
                    await outputStream.WriteAsync(segment);
                }
            }

            reader.AdvanceTo(buffer.End);

            if (result.IsCompleted) break;
        }

        await reader.CompleteAsync();
    }
}
