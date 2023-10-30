using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApiJiraRedmineBiblioteca.Procesos;

namespace WebApiJiraRedmineApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TicketsController : ControllerBase
    {
        public TicketsController(IConfiguration config)
        {
        }

        [AllowAnonymous]
        [HttpPost]
        public string GenerarToken()
        {
            return new ProcesosRedmine().GenerarToken();
        }

        [HttpPost]
        [Authorize]
        public string ObtenerTicketsRedmine()
        {
            return new ProcesosRedmine().ObtenerTicketsRedmine();
        }

        [HttpPost]
        [Authorize]
        public string ObtenerTicketsJira()
        {
            return new ProcesosJira().ObtenerTicketsJira();
        }
    }
}