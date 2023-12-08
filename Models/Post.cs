using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServerBlogManagement.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        // Foreign key
        public int BlogId { get; set; }

        // Navigation property
        public Blog Blog { get; set; }
    }
}
