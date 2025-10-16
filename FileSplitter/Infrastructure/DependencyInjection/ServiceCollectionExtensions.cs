using FileSplitter.Application.Contracts;
using FileSplitter.Application.Services;
using FileSplitter.Features.Csv;
using FileSplitter.Features.Excel;
using FileSplitter.Infrastructure.FileSystem;
using FileSplitter.Infrastructure.UserInterface;
using Microsoft.Extensions.DependencyInjection;

namespace FileSplitter.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileProcessingServices(this IServiceCollection services)
    {
        services.AddSingleton<IFilePathResolver, FilePathResolver>();
        services.AddSingleton<IUserInteraction, ConsoleUserInteraction>();
        
        services.AddTransient<IFileProcessor, CsvProcessor>();
        services.AddTransient<IFileProcessor, ExcelProcessor>();
        
        services.AddSingleton<IFileProcessorFactory, FileProcessorFactory>();
        services.AddSingleton<FileProcessingOrchestrator>();

        return services;
    }
}
