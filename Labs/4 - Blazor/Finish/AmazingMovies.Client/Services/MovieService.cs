using AmazingMovies.Models;
using Bogus;

namespace MyMovies.Services
{
    public class MovieService : IMovieService
    {
        public async Task<IQueryable<Movie>> GetMovies()
        {
            string[] genreNames = new[] { "Action", "Adult", "Adventure", "Animation", "Biography", "Comedy", "Crime", "Documentary", "Drama", "Family", "Fantasy", "Film Noir", "Game Show", "History", "Horror", "Musical", "Music", "Mystery", "News", "Reality-TV", "Romance", "Sci-Fi", "Short", "Sport", "Talk-Show", "Thriller", "War", "Western" };
            string[] posters = new[] { "/s1VzVhXlqsevi8zeCMG9A16nEUf.jpg", "/cvsXj3I9Q2iyyIo95AecSd1tad7.jpg", "/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg", "/3GrRgt6CiLIUXUtoktcv1g2iwT5.jpg", "/qNBAXBIQlnOThrVvA6mA2B5ggV6.jpg", "/ngl2FKBlU4fhbdsrtdom9LVLBXw.jpg", "/rzRb63TldOKdKydCvWJM8B6EkPM.jpg" };

            IEnumerable<Genre> genres = genreNames.Select(
                t => new Genre { Name = t });

            var faker = new Faker<Movie>()
                .RuleFor(m => m.Title, f => f.Random.Words(3))
                .RuleFor(m => m.Genre, f => f.PickRandom(genres))
                .RuleFor(m => m.ReleaseDate, f => f.Date.Past(50))
                .RuleFor(m => m.Poster, f => f.PickRandom(posters))
                .RuleFor(m => m.Plot, f => f.Rant.Review("movie"))
                .RuleFor(m => m.Price, f => f.Finance.Amount(5, 50));
            return faker.Generate(100).AsQueryable();
        }
    }
}