using Microsoft.AspNetCore.Mvc;

namespace NestorHub.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConnectionController : ControllerBase
    {
        [HttpGet]
        public bool Get()
        {
            return true;
        }
    }
}
