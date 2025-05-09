using Dapper;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MySql.Data.MySqlClient;

namespace MovieTicket_Backend.RepositoryInpl
{
    public class CinemaRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly IConfiguration _configuration;
        private readonly MySqlConnection connection;

        public CinemaRepository(DbConnectionFactory dbConnectionFactory, IConfiguration configuration)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _configuration = configuration;
            connection = _dbConnectionFactory.CreateConnection();
        }
        public async Task<List<Cinema>> GetAllCinemas()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var query = @"
            SELECT 
                c.cinema_id AS CinemaId, 
                c.cinema_name AS CinemaName, 
                c.location AS Location,
                cb.brand_name AS Brand,
                ct.city_name AS City
            FROM 
                cinema as c
            JOIN 
                city as ct ON c.city_id = ct.city_id
            JOIN 
                cinema_brand as cb on c.brand_id=cb.brand_id";
            var cinemas = await connection.QueryAsync<Cinema>(query);
            return cinemas.ToList();
        }
        public async Task<List<Cinema>> GetCinemasByBrandId(int brandId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var query = @"
            SELECT 
                c.cinema_id AS CinemaId, 
                c.cinema_name AS CinemaName, 
                c.location AS Location,
                cb.brand_name AS Brand,
                ct.city_name AS City
            FROM 
                cinema AS c
            JOIN 
                city AS ct ON c.city_id = ct.city_id
            JOIN 
                cinema_brand as cb on c.brand_id=cb.brand_id
            WHERE c.brand_id = @BrandId";
            var cinemas = await connection.QueryAsync<Cinema>(query, new { BrandId = brandId });
            return cinemas.ToList();
        }
        public async Task<List<Cinema>> GetCinemasByCityId(int cityId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var query = @"
            SELECT 
                c.cinema_id AS CinemaId, 
                c.cinema_name AS CinemaName, 
                c.location AS Location,
                cb.brand_name AS Brand,
                ct.city_name AS City
            FROM 
                cinema AS c
            JOIN 
                city AS ct ON c.city_id = ct.city_id
            JOIN 
                cinema_brand as cb on c.brand_id=cb.brand_id
            WHERE 
                c.city_id = @CityId";
            var cinemas = await connection.QueryAsync<Cinema>(query, new { CityId = cityId });
            return cinemas.ToList();
        }
        public async Task<List<Cinema>> GetCinemasByMovieId(int movieId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var query = @"
                SELECT 
                    c.cinema_id AS CinemaId, 
                    c.cinema_name AS CinemaName, 
                    c.location AS Location, 
                    c.city_id AS CityId, 
                    ct.city_name AS CityName
                FROM 
                    cinema AS c
                JOIN 
                    city AS ct ON c.city_id = ct.city_id
                JOIN 
                    screen AS s ON c.cinema_id = s.cinema_id
                JOIN 
                    showtime AS st ON s.screen_id = st.screen_id
                WHERE 
                    st.movie_id = @MovieId";
            var cinemas = await connection.QueryAsync<Cinema>(query, new { MovieId = movieId });
            return cinemas.ToList();
        }
    }
}
