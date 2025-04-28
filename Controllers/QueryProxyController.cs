using Microsoft.AspNetCore.Mvc;
using JobDescriptionAgent.Services;

namespace JobDescriptionAgent.Controllers
{
    [ApiController]
    [Route("api/queryproxy")]
    public class QueryProxyController : ControllerBase
    {
        private readonly IQueryProcessor _queryProcessor;
        public QueryProxyController(IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }

        [HttpPost]
        public async Task<IActionResult> Proxy([FromQuery] string job_id, [FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Invalid request data");
            if (string.IsNullOrWhiteSpace(job_id))
            {
                job_id = "0";
            }

            var apiContent = await _queryProcessor.QueryAsync(job_id, query);
            return Content(apiContent, "application/json");
        }
    }
}
