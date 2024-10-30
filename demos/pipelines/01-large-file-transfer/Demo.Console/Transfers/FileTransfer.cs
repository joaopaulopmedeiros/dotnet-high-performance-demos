using System.Buffers;
using System.IO.Pipelines;

namespace Demo.Console.Processors;

public class FileTransfer : IFileTransfer
{
    public async Task TransferAsync(string inputFilePath, string outputFilePath)
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
        const int minimumBufferSize = 1024 * 1024; //1MB

        while (true)
        {
            Memory<byte> memory = writer.GetMemory(minimumBufferSize);

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
