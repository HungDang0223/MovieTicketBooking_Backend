using Microsoft.EntityFrameworkCore;
using MimeKit;
using MovieTicket_Backend.Models;
using MySql.Data.MySqlClient;
using System.Data;
using static Azure.Core.HttpHeader;

namespace MovieTicket_Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}

        public DbSet<City> Cities { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }
        public DbSet<Ticket> Bookings { get; set; }
        public DbSet<BookingSnack> BookingSnacks { get; set; }
        public DbSet<BookingCombo> BookingCombos { get; set; }
        public DbSet<ShowingMovie> MovieShowings { get; set; }
        public DbSet<Screen> Screens { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Snack> Snacks { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ShowingSeat> ShowingSeats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeviceToken>().ToTable("device_token")
                .HasKey(dt => new { dt.UserId, dt.Token });

            modelBuilder.Entity<ShowingMovie>().ToTable("showing_movie")
                .HasKey(sm => new { sm.ShowingId, sm.MovieId });

            modelBuilder.Entity<ShowingMovie>().ToTable("showing_movie")
                .HasOne(sm => sm.Screen)
                .WithMany(s => s.MovieShowings)
                .HasForeignKey(sm => sm.ScreenId);

            modelBuilder.Entity<BookingSeat>().ToTable("booking_seat")
                .HasKey(bs => new { bs.BookingId, bs.SeatId });

            modelBuilder.Entity<BookingSnack>().ToTable("booking_snack")
                .HasKey(bs => new { bs.BookingId, bs.SnackId });

            modelBuilder.Entity<BookingCombo>().ToTable("booking_combo")
                .HasKey(bc => new { bc.BookingId, bc.ComboId });

            modelBuilder.Entity<ShowingSeat>().ToTable("showing_seat")
                .HasKey(ss => new { ss.ShowingId, ss.SeatId });

            modelBuilder.Entity<SeatPrice>().ToTable("seat_price")
                .HasKey(lp => new { lp.ShowingFormat, lp.SeatType });

            modelBuilder.Entity<ScreenRow>().ToTable("screen_row")
                .HasKey(sr => sr.RowId);

            modelBuilder.Entity<Screen>().ToTable("screen")
                .HasKey(s => s.ScreenId);

            modelBuilder.Entity<Snack>().ToTable("snack")
                .HasKey(s => s.SnackId);

            modelBuilder.Entity<Combo>().ToTable("combo")
                .HasKey(c => c.ComboId);

            modelBuilder.Entity<Discount>().ToTable("discount")
                .HasKey(d => d.DiscountId);

            modelBuilder.Entity<User>().ToTable("user")
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Payment>().ToTable("payment")
                .HasKey(p => p.PaymentId);

            modelBuilder.Entity<Ticket>().ToTable("ticket")
                .HasKey(t => t.BookingId);

            modelBuilder.Entity<SeatPrice>().ToTable("seat_price")
                .HasKey(sp => new { sp.ShowingFormat, sp.SeatType });

            modelBuilder.Entity<City>().ToTable("city")
                .HasKey(c => c.CityId);

            modelBuilder.Entity<Cinema>().ToTable("cinema")
                .HasKey(c => c.CinemaId);

            modelBuilder.Entity<Cinema>().ToTable("cinema")
                .HasOne(c => c.City)
                .WithMany(c => c.Cinemas)
                .HasForeignKey(c => c.CityId);

            modelBuilder.Entity<Brand>().ToTable("brand")
                .HasKey(cb => cb.BrandId);

            modelBuilder.Entity<Favourite>().ToTable("favourite")
                .HasKey(f => new { f.UserId, f.MovieId });


            // Additional configuration if needed  
            base.OnModelCreating(modelBuilder);
        }
    }
}
