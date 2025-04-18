using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly ILogger<DebugController> _logger;

        public DebugController(ILogger<DebugController> logger)
        {
            _logger = logger;
        }

        [HttpGet("token-info")]
        public IActionResult GetTokenInfo()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            
            return Ok(new
            {
                HasAuthHeader = !string.IsNullOrEmpty(authHeader),
                AuthHeader = authHeader,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                UserName = User.Identity?.Name,
                UserClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        [Authorize]
        [HttpGet("auth-check")]
        public IActionResult CheckAuth()
        {
            var userName = User.Identity?.Name;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("Auth check successful for user: {UserName}", userName);

            return Ok(new
            {
                Message = "Вы успешно авторизованы!",
                UserName = userName,
                UserId = userId,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
    }
} 