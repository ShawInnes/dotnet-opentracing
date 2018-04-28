using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Extensions.Configuration;
using App.Metrics.Health;
using App.Metrics.Health.Builder;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing;
using Petabridge.Tracing.Zipkin;

namespace Api
{
  public class Startup
  {
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
          .AddEnvironmentVariables();
      this.Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; private set; }

    // ConfigureServices is where you register dependencies. This gets
    // called by the runtime before the ConfigureContainer method, below.
    public void ConfigureServices(IServiceCollection services)
    {
      // Add services to the collection. Don't build or return
      // any IServiceProvider or the ConfigureContainer method
      // won't get called.
      services.AddMetricsTrackingMiddleware();
      services.AddMetricsEndpoints();
      services.AddHealthEndpoints();
      services.AddMetrics(ConfigureMetrics());
      services.AddHealth(ConfigureHealth());
      services.AddMetricsReportScheduler();
      services.AddMvc();
    }

    private IMetricsRoot ConfigureMetrics()
    {
      var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json")
              .Build();

      var metrics = new MetricsBuilder()
                  .Configuration.ReadFrom(configuration)
                  .OutputMetrics.AsJson()
                  .Build();

      return metrics;
    }

    private IHealthRoot ConfigureHealth()
    {
      var healthBuilder = new HealthBuilder();
      healthBuilder.HealthChecks.AddCheck(new SampleHealthCheck());
      return healthBuilder.Build();
    }

    // ConfigureContainer is where you can register things directly
    // with Autofac. This runs after ConfigureServices so the things
    // here will override registrations made in ConfigureServices.
    // Don't build the container; that gets done for you. If you
    // need a reference to the container, you need to use the
    // "Without ConfigureContainer" mechanism shown later.
    public void ConfigureContainer(ContainerBuilder builder)
    {
      // builder.RegisterModule(new AutofacModule());
      // builder.RegisterInstance<IMetrics>(ConfigureMetrics()).SingleInstance();
      // builder.RegisterInstance<IHealth>(ConfigureHealth()).SingleInstance();

      var url = "http://localhost:9411";
      var tracer = new ZipkinTracer(new ZipkinTracerOptions(url, "api", debug: true));

      builder.RegisterInstance<ITracer>(tracer);
    }

    // Configure is where you add middleware. This is called after
    // ConfigureContainer. You can use IApplicationBuilder.ApplicationServices
    // here if you need to resolve things from the container.
    public void Configure(
      IApplicationBuilder app)
    {
      app.UseMetricsAllMiddleware();
      app.UseMetricsAllEndpoints();
      app.UseHealthAllEndpoints();
      app.UseMvc();
    }
  }
}
