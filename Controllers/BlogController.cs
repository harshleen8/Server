using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ServerBlogManagement.Data;
using ServerBlogManagement.Models;

[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<BlogController> _logger;

    public BlogController(AppDbContext context, ILogger<BlogController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/Blog
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Blog>>> GetBlogs()
    {
        return await _context.Blogs.ToListAsync();
    }

    // GET: api/Blog/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Blog>> GetBlog(int id)
    {
        Blog? blog = await _context.Blogs.FindAsync(id);
        return blog is null ? NotFound() : blog;
    }

    [HttpPost]
    public async Task<ActionResult> PostBlog([FromBody] BlogInputModel blogInput)
    {
        // Map the input model to the entity
        var blog = new Blog
        {
            Title = blogInput.Title,
            Posts = blogInput.Posts?.Select(postInput => new Post
            {
                Title = postInput.PostTitle,
                Content = postInput.Content
            }).ToList()
        };

        // Add the blog to the context
        _context.Blogs.Add(blog);

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Map the entity to the DTO
        var blogDto = new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Posts = blog.Posts?.Select(post => new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content
            }).ToList()
        };

        // Return the DTO
        return CreatedAtAction(nameof(GetBlog), new { id = blog.Id }, blogDto);
    }



    [HttpPut("{id}")]
    public async Task<IActionResult> PutBlog(int id, Blog updatedBlog)
    {
        if (id != updatedBlog.Id)
        {
            return BadRequest("Mismatched IDs");
        }

        // Update the main blog entity
        _context.Entry(updatedBlog).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Blogs.Any(e => e.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        // Update related posts
        foreach (var updatedPost in updatedBlog.Posts)
        {
            _context.Entry(updatedPost).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }


    // DELETE: api/Blog/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBlog(int id)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null)
        {
            return NotFound();
        }

        _context.Blogs.Remove(blog);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BlogExists(int id)
    {
        return _context.Blogs.Any(e => e.Id == id);
    }
}
