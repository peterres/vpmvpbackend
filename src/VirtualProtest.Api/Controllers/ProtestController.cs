using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using VirtualProtest.Core.Interfaces;
using VirtualProtest.Core.Models;
using VirtualProtest.Services;

namespace VirtualProtest.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProtestController : ControllerBase
    {
        private readonly IProtestService _protestService;

        public ProtestController(IProtestService protestService)
        {
            _protestService = protestService;
        }

        // GET: /protests
        [HttpGet]
        public ActionResult<IEnumerable<Protest>> GetProtests()
        {
            var protests = _protestService.GetAllProtests();
            return Ok(protests);
        }

        // POST: /protests
        [HttpPost]
        public ActionResult<Protest> CreateProtest([FromBody] Protest protest)
        {
            var createdProtest = _protestService.CreateProtest(protest);
            return CreatedAtAction(nameof(GetProtest), new { id = createdProtest.Id }, createdProtest);
        }

        // GET: /protests/{id}
        [HttpGet("{id}")]
        public ActionResult<Protest> GetProtest(Guid id)
        {
            var protest = _protestService.GetProtestById(id);

            if (protest == null)
            {
                return NotFound();
            }

            // just send empty list for now, we will need separate view model
            protest.Participants = new List<Participant>();

            return Ok(protest);
        }
    }
}