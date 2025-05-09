using Dapper;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models.ModelDTOs;

namespace MovieTicket_Backend.RepositoryInpl
{
    public class MovieShowingRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly IConfiguration _configuration;
        public MovieShowingRepository(DbConnectionFactory dbConnectionFactory, IConfiguration configuration)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Get all movie showings by movie id and date and cinema id
        /// </summary>
        /// <param name="movieId"></param>
        /// <param name="date"></param>
        /// <param name="cinemaId"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<MovieShowingDTO>>> GetGroupedShowings(int movieId, DateOnly date, int cinemaBrandId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                SELECT 
                    ms.start_time AS StartTime,
                    ms.end_time AS EndTime,
                    ms.showing_date AS ShowingDate,
                    ms.showing_format AS ShowingFormat,
                    c.cinema_name AS CinemaName,
                    s.screen_name AS ScreenName
                FROM showing_movie ms
                JOIN movie m ON ms.movie_id = m.movie_id
                JOIN screen s ON ms.screen_id = s.screen_id
                JOIN cinema c ON s.cinema_id = c.cinema_id
                WHERE ms.movie_id = @MovieId 
                  AND c.brand_id = @CinemaBrandId 
                  AND ms.showing_date = @Date";
            var parameters = new
            {
                MovieId = movieId,
                CinemaBrandId = cinemaBrandId,
                Date = date.ToDateTime(TimeOnly.MinValue)
            };

            var showings = await connection.QueryAsync<MovieShowingDTO>(query, parameters);

            var grouped = showings
                .GroupBy(s => s.CinemaName)
                .ToDictionary(g => g.Key, g => g.ToList());

            return grouped;
        }

    }
}
