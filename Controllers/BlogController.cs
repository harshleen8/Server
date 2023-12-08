using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<ActionResult<IEnumerable<Blog>>> GetBlogs()
    {
        return await _context.Blogs.ToListAsync();
    }

    // GET: api/Blog/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Blog>> GetBlog(int id)
    {
        var blog = await _context.Blogs.Include(b => b.Posts).FirstOrDefaultAsync(b => b.Id == id);

        if (blog == null)
        {
            return NotFound();
        }

        return blog;
    }

    [HttpPost]
    public async Task<ActionResult<Blog>> PostBlog([FromBody] Blog blog)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBlog), new { id = blog.Id }, blog);
        }
        catch (Exception ex)
        {
            // Log the exception using a logging framework
            _logger.LogError(ex, "Error creating a new blog");

            // Return a generic error response
            return StatusCode(500, "Internal Server Error");
        }
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
