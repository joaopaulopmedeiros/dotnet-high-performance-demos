namespace Demo.Console.Processors;

public interface ICsvProcessor
{
    public Task ProcessAsync(string inputFilePath, string outputFilePath);
}
