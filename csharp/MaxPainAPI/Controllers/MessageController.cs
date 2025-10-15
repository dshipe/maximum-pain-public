using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MaxPainAPI.Controllers
{
    //[Route("/")]
    [Route("api/[controller]")]
    public class MessageController : Controller
    {
        private readonly AwsContext _awsContext;
        private readonly ILoggerService _logger;

        public MessageController(
            AwsContext awsContext,
            ILoggerService loggerService)
        {
            _awsContext = awsContext;
            _logger = loggerService;
        }

        // GET: api/Message
        [HttpGet]
        public async Task<JsonResult> Get()
        {
            return Json(await _awsContext.Message.OrderByDescending(x => x.Id).ToListAsync());
        }

        // GET: api/Message/1
        [HttpGet("{id}")]
        public async Task<JsonResult> Get(long id)
        {
            var msg = await _awsContext.Message.FindAsync(id);

            if (msg == null)
            {
                return Json(NotFound());
            }

            return Json(msg);
        }


        [HttpGet("Truncate")]
        public async Task<JsonResult> Truncate()
        {
            _awsContext.Database.ExecuteSqlRaw("TRUNCATE TABLE [Message]");
            return Json(await _awsContext.Message.OrderByDescending(x => x.Id).ToListAsync());
        }

        [HttpGet("Create")]
        public async Task<JsonResult> Create(string subject, string body)
        {
            await _logger.InfoAsync(subject, body);
            return Json(await _awsContext.Message.OrderByDescending(x => x.Id).ToListAsync());
        }
    }
}
