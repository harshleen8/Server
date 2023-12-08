using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;

namespace ServerBlogManagement.Data
{
    public class BlogDataCsv
    {
        [Name("Id")]
        public int Id { get; set; }

        [Name("Title")]
        public string Title { get; set; }

        [Name("Posts")]
        public List<string> Posts { get; set; }
    }
}
