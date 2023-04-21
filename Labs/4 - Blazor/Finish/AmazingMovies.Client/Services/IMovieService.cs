using AmazingMovies.Models;

namespace MyMovies.Services
{
    public interface IMovieService
    {
        Task<IQueryable<Movie>> GetMovies();
    }
}