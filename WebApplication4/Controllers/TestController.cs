using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [Authorize]
        [HttpGet("private")]
        public IActionResult GetPrivate()
        {
            var userName = User.Identity?.Name;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Ok(new { 
                Message = "Это защищенный эндпоинт! Вы авторизованы.",
                UserName = userName,
                UserId = userId,
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [HttpGet("public")]
        public IActionResult GetPublic()
        {
            return Ok(new { Message = "Это публичный эндпоинт! Доступен всем." });
        }
    }
}