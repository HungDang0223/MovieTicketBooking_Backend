using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.Repositories
{
    public interface IUserRepository
    {
        Task<string?> CreateUser(User user);
        Task<User> GetUserById(string id);
        Task<User> GetUserByEmailPhone(string emailPhone);
        Task<bool> UpdateUser(User user);
        Task<bool> DeleteUser(string id);
        Task<List<User>> GetAllUsers();
        Task<(string AccessToken, string RefreshToken, User user)> AuthenticateUser(string emailPhone, string password);
        Task<List<Ticket>> GetUserBookings(string userId);
    }
}
