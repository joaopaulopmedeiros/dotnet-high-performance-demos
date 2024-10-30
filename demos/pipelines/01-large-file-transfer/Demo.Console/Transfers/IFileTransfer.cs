namespace Demo.Console.Processors;

public interface IFileTransfer
{
    public Task TransferAsync(string inputFilePath, string outputFilePath);
}
