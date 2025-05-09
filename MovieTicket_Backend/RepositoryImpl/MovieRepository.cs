using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Models.ModelDTOs;
using MySql.Data.MySqlClient;

namespace MovieTicket_Backend.RepositoryInpl
{
    public class MovieRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly IConfiguration _configuration;
        private readonly MySqlConnection connection;

        public MovieRepository(DbConnectionFactory dbConnectionFactory, IConfiguration configuration)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _configuration = configuration;
            connection = _dbConnectionFactory.CreateConnection();
        }
        public async Task<List<Movie>> GetAllMovies()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var query = @"
            SELECT 
                mv.movie_id AS MovieId, 
                mv.title AS Title, 
                mv.release_date AS ReleaseDate, 
                mv.duration AS Duration, 
                mv.rating AS Rating, 
                mv.synopsis AS Synopsis, 
                mv.poster_url AS PosterUrl, 
                mv.trailer_url AS TrailerUrl, 
                mv.censor AS Censor, 
                mv.cast AS Cast, 
                mv.directors AS Directors, 
                mv.genre AS Genre, 
                mv.showing_date AS ShowingDate, 
                mv.end_date AS EndDate, 
                mv.showing_status AS ShowingStatus, 
                mv.is_special AS IsSpecial, 
                COUNT(fa.movie_id) AS FavouritesCount
            FROM 
                movie AS mv
            LEFT JOIN 
                favourite AS fa 
            ON 
                mv.movie_id = fa.movie_id
            GROUP BY 
                mv.movie_id, mv.title, mv.release_date, mv.duration, mv.rating, mv.synopsis, 
                mv.poster_url, mv.trailer_url, mv.censor, mv.cast, mv.directors, mv.genre, 
                mv.showing_date, mv.end_date, mv.showing_status, mv.is_special";

            var movies = await connection.QueryAsync<Movie>(query);
            return movies.ToList();
        }
        public async Task<Movie> GetMovieById(int movieId, string? userId=null)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
                    SELECT 
                        mv.title AS Title,
                        mv.release_date AS ReleaseDate,
                        mv.duration AS Duration,
                        mv.rating AS Rating,
                        mv.synopsis AS Synopsis,
                        mv.poster_url AS PosterUrl,
                        mv.trailer_url AS TrailerUrl,
                        mv.censor AS Censor,
                        mv.cast AS Cast,
                        mv.directors AS Directors,
                        mv.genre AS Genre,
                        mv.showing_date AS ShowingDate,
                        mv.end_date AS EndDate,
                        mv.showing_status AS ShowingStatus,
                        mv.is_special AS IsSpecial,
                        (SELECT COUNT(*) FROM favourite WHERE movie_id = mv.movie_id) AS FavouritesCount,
                        (SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END FROM favourite WHERE movie_id = mv.movie_id AND user_id = @UserId) AS IsFavourited
                    FROM movie mv
                    WHERE mv.movie_id = @MovieId;";

            var parameters = new { MovieId = movieId, UserId = userId };

            var movie = await connection.QueryFirstAsync<Movie>(sql, parameters);

            return movie;
        }

        public async Task<bool> AddMovie(Movie movie)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"INSERT INTO movie (title, release_date, duration, rating, synopsis, poster_url, trailer_url, censor, cast, directors, genre, showing_date, end_date, is_special)
                             VALUES (@Title, @ReleaseDate, @Duration, @Rating, @Synopsis, @PosterUrl, @TrailerUrl, @Censor, @Cast, @Directors, @Genre, @ShowingDate, @EndDate, @IsSpecial)";
            var parameters = new { movie };
            var rowEffect = await connection.ExecuteAsync(query, parameters);
            return rowEffect > 0;
        }
        public async Task<bool> UpdateMovieAsync(Movie movie)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"UPDATE movie SET title = @Title, release_date = @ReleaseDate, duration = @Duration, rating = @Rating, synopsis = @Synopsis,
                             poster_url = @PosterUrl, trailer_url = @TrailerUrl, censor = @Censor, cast = @Cast, directors = @Directors,
                             genre = @Genre, showing_date = @ShowingDate, end_date = @EndDate, is_special = @IsSpecial
                             WHERE movie_id = @MovieId";
            var parameters = new { movie };
            var rowEffect = await connection.ExecuteAsync(query, parameters);
            return rowEffect > 0;
        }
        public async Task<int> DeleteMovie(int id)
        {
            var movie = await GetMovieById(id);
            if (movie != null)
            {
                using var connection = _dbConnectionFactory.CreateConnection();
                string query = "DELETE FROM movie WHERE movie_id = @Id";
                var parameters = new { Id = id };
                var rowEffect = await connection.ExecuteAsync(query, parameters);
                return rowEffect;
            }
            return -1;
        }

        public async Task<List<Movie>> GetMoviesByShowingStatus(string status)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var movies = await connection.QueryAsync<Movie>("SELECT * FROM movie WHERE showing_status = @Status", new { Status = status });
            return movies.ToList();
        }

        public async Task<List<Review>> GetMovieReviews(int movieId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var reviews = await connection.QueryAsync<Review>("SELECT * FROM review WHERE movie_id = @MovieId", new { MovieId = movieId });
            return reviews.ToList();
        }
    }
}
