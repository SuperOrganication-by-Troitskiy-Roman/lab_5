// Models/PostResponseDto.cs
namespace WebApplication4.Models
{
    public class PostResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; }
        public bool IsPublished { get; set; }
    }
}