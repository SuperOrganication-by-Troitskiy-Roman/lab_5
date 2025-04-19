using WebApplication4.Data;
using WebApplication4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebApplication4.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PostsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/posts (только опубликованные посты)
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPublishedPosts()
        {
            return await _context.Posts
                .Where(p => p.IsPublished)
                .Include(p => p.Author)
                .Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    AuthorName = p.Author.UserName,
                    IsPublished = p.IsPublished
                })
                .ToListAsync();
        }

        // GET: api/posts/my (посты текущего пользователя)
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetMyPosts()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);

            return await _context.Posts
                .Where(p => p.Author.UserName == userName)
                .Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    AuthorName = p.Author.UserName,
                    IsPublished = p.IsPublished
                })
                .ToListAsync();
        }

        // GET: api/posts/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PostResponseDto>> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Только автор или админ может видеть неопубликованный пост
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (!post.IsPublished && post.Author.UserName != userName && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return new PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                AuthorName = post.Author.UserName,
                IsPublished = post.IsPublished
            };
        }

        // POST: api/posts
        [HttpPost]
        public async Task<ActionResult<PostResponseDto>> CreatePost(PostDto postDto)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _userManager.FindByNameAsync(userName);

            var post = new Post
            {
                Title = postDto.Title,
                Content = postDto.Content,
                AuthorId = user.Id,
                IsPublished = postDto.IsPublished
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPost), new { id = post.Id },
                new PostResponseDto
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    CreatedAt = post.CreatedAt,
                    AuthorName = user.UserName,
                    IsPublished = post.IsPublished
                });
        }

        // PUT: api/posts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, PostDto postDto)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _userManager.FindByNameAsync(userName);
            
            if (post.AuthorId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            post.Title = postDto.Title;
            post.Content = postDto.Content;
            post.IsPublished = postDto.IsPublished;
            post.UpdatedAt = DateTime.UtcNow;

            _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/posts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _userManager.FindByNameAsync(userName);
            
            if (post.AuthorId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}