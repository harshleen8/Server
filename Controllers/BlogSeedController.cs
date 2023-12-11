using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ServerBlogManagement.Data;
using ServerBlogManagement.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebAPI;

namespace ServerBlogManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogSeedController : ControllerBase
    {
        private readonly UserManager<BlogManagement> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly ILogger<BlogSeedController> _logger;
        private readonly string _pathName;

        public BlogSeedController(
            UserManager<BlogManagement> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            AppDbContext context,
            IHostEnvironment environment,
            ILogger<BlogSeedController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _pathName = Path.Combine(environment.ContentRootPath, "Data/blogdata.csv");
        }

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

                        bool allExistingBlogsSatisfyCondition = await _context.Blogs.AllAsync(existingBlog => existingBlog.Title.Length < 50);

                        blogsByTitle.Add(record.Title, blog);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Imported {blogsByTitle.Count} blogs successfully.");

                return new JsonResult(blogsByTitle.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing blogs from CSV");

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {InnerExceptionMessage}", ex.InnerException.Message);
                }

                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }


        }

        [HttpPost("Users")]
        public async Task<IActionResult> ImportUsers()
        {
            const string roleUser = "RegisteredUser";
            const string roleAdmin = "Administrator";

            if (await _roleManager.FindByNameAsync(roleUser) is null)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleUser));
            }

            if (await _roleManager.FindByNameAsync(roleAdmin) is null)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleAdmin));
            }

            List<BlogManagement> addedUserList = new();
            (string name, string email) = ("admin", "admin@email.com");

            if (await _userManager.FindByNameAsync(name) is null)
            {
                BlogManagement userAdmin = new()
                {
                    UserName = name,
                    Email = email,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                await _userManager.CreateAsync(userAdmin, _configuration["DefaultPasswords:Administrator"]
                    ?? throw new InvalidOperationException());

                await _userManager.AddToRolesAsync(userAdmin, new[] { roleUser, roleAdmin });
                userAdmin.EmailConfirmed = true;
                userAdmin.LockoutEnabled = false;
                addedUserList.Add(userAdmin);
            }

            (string registeredName, string registeredEmail) = ("user", "user@email.com");

            if (await _userManager.FindByNameAsync(registeredName) is null)
            {
                BlogManagement user = new()
                {
                    UserName = registeredName,
                    Email = registeredEmail,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                await _userManager.CreateAsync(user, _configuration["DefaultPasswords:RegisteredUser"]
                    ?? throw new InvalidOperationException());

                await _userManager.AddToRoleAsync(user, roleUser);
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                addedUserList.Add(user);
            }

            if (addedUserList.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new
            {
                addedUserList.Count,
                Users = addedUserList
            });
        }
    }
}
