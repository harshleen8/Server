namespace ServerBlogManagement.Models
{
    public class BlogDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<PostDto> Posts { get; set; }
    }
}
