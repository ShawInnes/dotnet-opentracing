using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Health;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;
using Petabridge.Tracing.Zipkin;

namespace Api.Controllers
{
  [Route("api/[controller]")]
  public class ValuesController : Controller
  {
    private readonly IMetrics metrics;
    private readonly IHealthRoot health;
    private readonly ITracer tracer;

    public ValuesController(IMetrics metrics, IHealthRoot health, ITracer tracer)
    {
      this.metrics = metrics;
      this.health = health;
      this.tracer = tracer;
    }

    // GET api/values
    [HttpGet]
    public IEnumerable<string> Get()
    {
      var sb = tracer.BuildSpan("get-values").WithTag("empty", true);
      var span = sb.Start();
      metrics.Measure.Counter.Increment(MetricsRegistry.RequestCounter);
      using (metrics.Measure.Timer.Time(MetricsRegistry.SampleTimer, "values/get"))
      {
        span.Finish();
        return new string[] { "value1", "value2" };
      }
    }

    [HttpGet("health")]
    public async Task<HealthCheckStatus> GetHealth()
    {
      var value = await health.HealthCheckRunner.ReadAsync();
      return value.Status;
    }

    // GET api/values/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
      return "value";
    }

    // POST api/values
    [HttpPost]
    public void Post([FromBody]string value)
    {
    }

    // PUT api/values/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody]string value)
    {
    }

    // DELETE api/values/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
