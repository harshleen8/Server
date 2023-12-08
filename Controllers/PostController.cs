using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerBlogManagement.Data;
using ServerBlogManagement.Models;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly AppDbContext _context;

    public PostController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Post
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
    {
        return await _context.Posts.ToListAsync();
    }

    // GET: api/Post/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPost(int id)
    {
        var post = await _context.Posts.Include(p => p.Blog).FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        return post;
    }

    [HttpPost]
    public async Task<ActionResult<Post>> PostPost(Post post)
    {
        if (post.Blog != null && post.Blog.Id > 0)
        {
            // If Blog property is set, use its Id
            post.BlogId = post.Blog.Id;
            // Make sure the related Blog is not attached to the context to avoid conflicts
            _context.Entry(post.Blog).State = EntityState.Detached;
            // Set the Blog property to null to avoid conflicts
            post.Blog = null;
        }

        _context.Posts.Attach(post);  // Attach the post to the context
        _context.Entry(post).State = EntityState.Added;  // Set the state to Added

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }




    [HttpPut("{id}")]
    public async Task<IActionResult> PutPost(int id, Post post)
    {
        if (id != post.Id)
        {
            return BadRequest();
        }

        _context.Entry(post).State = EntityState.Modified;

        // Check if the associated blog exists and update its properties
        if (_context.Blogs.Any(b => b.Id == post.BlogId))
        {
            _context.Entry(post.Blog).State = EntityState.Modified;
        }

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



    // DELETE: api/Post/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
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
