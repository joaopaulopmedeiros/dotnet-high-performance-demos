namespace Demo.Console;

public interface ICsvProcessor
{
    public Task ProcessAsync(string inputFilePath, string outputFilePath);
}
