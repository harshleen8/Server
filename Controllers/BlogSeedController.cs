using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerBlogManagement.Data;
using ServerBlogManagement.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServerBlogManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogSeedController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BlogSeedController> _logger;
        private readonly string _pathName;

        public BlogSeedController(AppDbContext context, ILogger<BlogSeedController> logger)
        {
            _context = context;
            _logger = logger;
            _pathName = Path.Combine(Directory.GetCurrentDirectory(), "Data/blogdata.csv");
        }

        // POST: api/BlogSeed
        [HttpPost]
        public async Task<IActionResult> ImportBlogs()
        {
            try
            {
                _logger.LogInformation("ImportBlogs method started.");

                Dictionary<string, Blog> blogsByTitle = await _context.Blogs
                    .AsNoTracking().ToDictionaryAsync(x => x.Title, StringComparer.OrdinalIgnoreCase);

                CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    HeaderValidated = null
                };

                using (StreamReader reader = new StreamReader(_pathName))
                using (CsvReader csv = new CsvReader(reader, config))
                {
                    IEnumerable<BlogDataCsv> records = csv.GetRecords<BlogDataCsv>();
                    foreach (BlogDataCsv record in records)
                    {
                        if (blogsByTitle.ContainsKey(record.Title))
                        {
                            _logger.LogWarning($"Blog with title '{record.Title}' already exists. Skipping.");
                            continue;
                        }

                        Blog blog = new Blog
                        {
                            Title = record.Title,
                            Posts = record.Posts?.Select(post => new Post { Title = post }).ToList()
                        };

                        // Your condition for checking existing blogs
                        bool allExistingBlogsSatisfyCondition = await _context.Blogs.AllAsync(existingBlog => existingBlog.Title.Length < 50);

                        if (allExistingBlogsSatisfyCondition)
                        {
                            // Your logic when all existing blogs satisfy the condition
                        }

                        blogsByTitle.Add(record.Title, blog);
                    }
                }

                await _context.Blogs.AddRangeAsync(blogsByTitle.Values);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Imported {blogsByTitle.Count} blogs successfully.");

                return new JsonResult(blogsByTitle.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing blogs from CSV");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
