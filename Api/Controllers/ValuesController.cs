using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Health;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
  [Route("api/[controller]")]
  public class ValuesController : Controller
  {
    private readonly IMetrics metrics;

    public ValuesController(IMetrics metrics)
    {
      this.metrics = metrics;
    }

    // GET api/values
    [HttpGet]
    public IEnumerable<string> Get()
    {
      metrics.Measure.Counter.Increment(MetricsRegistry.RequestCounter);
      using (metrics.Measure.Timer.Time(MetricsRegistry.SampleTimer, "values/get"))
      {
        return new string[] { "value1", "value2" };
      }
    }

    [HttpGet("health")]
    public string GetHealth()
    {
      return "eh?";
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
