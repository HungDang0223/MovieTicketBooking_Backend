using MovieTicket_Backend.ModelDTOs;
using MovieTicket_Backend.Models;
using static MovieTicket_Backend.ModelDTOs.ModelRequests;

namespace MovieTicket_Backend.Repositories
{
    public interface IUserRepository
    {
        Task<string?> CreateUser(UserDTO user);
        Task<User> GetUserById(string id);
        Task<User> GetUserByEmailPhone(string emailPhone);
        Task<bool> UpdateUserInfo(User user);
        Task<bool> UpdateUserPassword(UpdatePasswordRequest request);
        Task<bool> DeleteUser(string id);
        Task<List<User>> GetAllUsers();
        Task<(string AccessToken, string RefreshToken, User user)> AuthenticateUser(string emailPhone, string password);
        Task<List<Ticket>> GetUserBookings(string userId);
    }
}
