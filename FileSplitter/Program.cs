using FileSplitter.Application.Services;
using FileSplitter.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddFileProcessingServices();
    })
    .Build();

try
{
    var orchestrator = host.Services.GetRequiredService<FileProcessingOrchestrator>();
    await orchestrator.ExecuteAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}

