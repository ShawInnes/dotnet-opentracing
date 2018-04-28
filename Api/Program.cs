using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Extensions.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Autofac.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using App.Metrics.Health.Builder;
using App.Metrics.AspNetCore.Health;

namespace Api
{
  public class Program
  {
    public static int Main(string[] args)
    {
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.LiterateConsole()
        .CreateLogger();

      try
      {
        var host = new WebHostBuilder()
         .UseKestrel()
         .ConfigureServices(services => services.AddAutofac())
         .UseContentRoot(Directory.GetCurrentDirectory())
         .UseIISIntegration()
         .UseStartup<Startup>()
         .UseSerilog()
         .Build();
         
        host.Run();
        return 0;
      }
      catch (Exception ex)
      {
        Log.Fatal(ex, "Host terminated unexpectedly");
        return 1;
      }
      finally
      {
        Log.CloseAndFlush();
      }
    }
  }
}