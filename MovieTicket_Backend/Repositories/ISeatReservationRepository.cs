using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.Repositories
{
    public interface ISeatReservationRepository
    {
        Task<(bool Success, string Message)> ReserveSeatAsync(ReserveSeatRequest request);
        Task<bool> ConfirmReservationAsync(int screeningId, int seatId, string userId);
        Task<bool> CancelReservationAsync(int screeningId, int seatId, string userId);
    }
}
