using MovieTicket_Backend.ModelDTOs;
using MovieTicket_Backend.Models;
using static MovieTicket_Backend.ModelDTOs.ModelRequests;

namespace MovieTicket_Backend.Repositories
{
    public interface IBookingRepository
    {
        Task<string?> CreateBooking(BookingRequestDTO bookingRequest);
        Task<BookingDetailDTO?> GetBookingDetails(int bookingId);
    }
}
