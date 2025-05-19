using MovieTicket_Backend.Models;
using MovieTicket_Backend.Models.ModelDTOs;
using static MovieTicket_Backend.Models.ModelDTOs.ModelRequests;

namespace MovieTicket_Backend.Repositories
{
    public interface IBookingRepository
    {
        Task<string?> CreateBooking(BookingRequestDTO bookingRequest);
        Task<BookingDetailDTO?> GetBookingDetails(int bookingId);
    }
}
