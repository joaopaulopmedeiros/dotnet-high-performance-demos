using Demo.Console.Processors;

const string inputFilePath = "C://tmp/reports/input/custom_1988_2020.csv";
const string outputFilePath = "C://tmp/reports/output/custom_1988_2020.csv";

await new CsvPipelineProcessor().ProcessAsync(inputFilePath, outputFilePath);

Console.WriteLine($"CSV file available at {outputFilePath}");
