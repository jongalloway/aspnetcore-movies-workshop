using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesMovie.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<NewsItem> RecentNews { get; set; }

        public void OnGet()
        {
            {
                var faker = new Faker<NewsItem>()
                    .RuleFor(m => m.Title, f => f.Random.Words(5))
                    .RuleFor(m => m.Summary, f => f.Lorem.Sentences(3))
                    .RuleFor(m => m.ImageUrl, f => f.Image.LoremFlickrUrl(300,300,"movie"))
                    .RuleFor(m => m.Date, f => f.Date.RecentDateOnly(50))
                    .RuleFor(m => m.Url, f => "https://devblogs.microsoft.com/dotnet/");
                RecentNews = faker.Generate(10);
            }
        }
    }

    public class NewsItem
    {
        public string Title { get; set; } = default!;
        public string Summary { get; set; } = default!;
        public string Url { get; set; } = default!;
        public string ImageUrl { get; set; } = default!;
        public DateOnly Date { get; set; } = default!;
    }
}