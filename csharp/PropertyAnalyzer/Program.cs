using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PropertyAnalyser.Infrastructure;
using PropertyAnalyser.Services;

namespace PropertyAnalyser
{
    public class Program
    {
        // Main is now async
        public static async Task Main(string[] args)
        {
            // 1. Validate Command-Line Arguments
            if (args.Length != 2)
            {
                Console.WriteLine("Error: Invalid arguments.");
                Console.WriteLine("Usage: dotnet run <input_file_path> <output_file_path>");
                return;
            }

            string inputFile = args[0];
            string outputFile = args[1];

            // 2. Set up the .NET Generic Host and DI Container
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register our services with the DI container
                    // "When someone asks for IAnalysisService, create a new AnalysisService"
                    services.AddTransient<IAnalysisService, AnalysisService>();
                    
                    // "When someone asks for ICsvLoader, create a new CsvLoader"
                    services.AddTransient<ICsvLoader, CsvLoader>();

                    // We register our main application logic as a "hosted service"
                    services.AddHostedService<AppHost>();
                })
                .Build();

            // 3. Run the application
            await host.RunAsync();
        }

        // We've moved the application logic into its own class
        // This class will be created by the DI container
        private class AppHost : IHostedService
        {
            private readonly IAnalysisService _analysisService;
            private readonly ICsvLoader _loader;
            private readonly IHostApplicationLifetime _appLifetime;

            // 4. Constructor Injection:
            // The DI container "injects" the services we need into the constructor.
            public AppHost(
                IAnalysisService analysisService, 
                ICsvLoader loader,
                IHostApplicationLifetime appLifetime)
            {
                _analysisService = analysisService;
                _loader = loader;
                _appLifetime = appLifetime;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                // Get the file paths from the host configuration
                var args = Environment.GetCommandLineArgs();
                string inputFile = args[1];
                string outputFile = args[2];

                try
                {
                    Console.WriteLine("Starting analysis...");

                    // 5. Use the Injected Services
                    Console.WriteLine($"Loading data from {inputFile}...");
                    var listings = _loader.LoadListings(inputFile);
                    Console.WriteLine($"Loaded {listings.Count:N0} records.");

                    var stopwatch = Stopwatch.StartNew();
                    var topSuburbs = _analysisService.AnalyzeProperties(listings);
                    stopwatch.Stop();
                    
                    Console.WriteLine($"✅ Analysis complete in {stopwatch.ElapsedMilliseconds} ms.");

                    var report = new { TopSuburbs = topSuburbs };
                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(report, jsonOptions);

                    File.WriteAllText(outputFile, jsonString);

                    Console.WriteLine($"✅ Successfully wrote report to {outputFile}");

                    // Get the peak memory (RAM) usage for this process
                    var peakMemoryBytes = Process.GetCurrentProcess().PeakWorkingSet64;
                    var peakMemoryMb = peakMemoryBytes / 1024.0 / 1024.0;
                    Console.WriteLine($"✅ Peak memory usage: {peakMemoryMb:F2} MB");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"Error: The file '{inputFile}' was not found.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                }
                finally
                {
                    // Tell the host to shut down
                    _appLifetime.StopApplication();
                }
                
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}