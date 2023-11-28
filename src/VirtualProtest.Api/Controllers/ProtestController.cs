using Microsoft.AspNetCore.Mvc;
using VirtualProtest.Core.Models;

namespace VirtualProtest.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProtestController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ProtestController> _logger;

        public ProtestController(ILogger<ProtestController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetProtest")]
        public ProtestBase Get()
        {
            return  new ProtestBase
            {
                Date = DateTime.Now.AddDays(Random.Shared.Next(3, 24)),
                Title = Summaries[Random.Shared.Next(Summaries.Length)]
            };
        }
    }
}
