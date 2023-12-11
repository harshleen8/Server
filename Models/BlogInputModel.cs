using System.ComponentModel.DataAnnotations;

namespace ServerBlogManagement.Models
{
    public class BlogInputModel
    {
        [Required]
        public string Title { get; set; }

        public List<PostInputModel> Posts { get; set; }
    }

    public class PostInputModel
    {
        [Required]
        public string PostTitle { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int BlogId { get; set; }
    }

}
