using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace MovieTicket_Backend.Services
{
    public interface INotificationService
    {
        Task SendNewMovieNotification(string movieTitle, string description, List<string> userTokens);
        Task SendPaymentSuccessNotification(string movieTitle, DateTime showtime, string ticketInfo, string userToken);
    }

    public class FCMNotificationService : INotificationService
    {
        private readonly FirebaseMessaging _messaging;

        public FCMNotificationService(FirebaseApp app)
        {
            _messaging = FirebaseMessaging.GetMessaging(app);
        }

        public async Task SendNewMovieNotification(string movieTitle, string description, List<string> userTokens)
        {
            if (userTokens.Count == 0)
                return;

            var message = new MulticastMessage()
            {
                Notification = new Notification
                {
                    Title = $"New Movie: {movieTitle}",
                    Body = description
                },
                Data = new Dictionary<string, string>()
               {
                   { "type", "new_movie" },
                   { "movieId", movieTitle.GetHashCode().ToString() }
               },
                Tokens = userTokens
            };

            var response = await _messaging.SendEachForMulticastAsync(message);
            Console.WriteLine($"Successfully sent {response.SuccessCount} notifications.");
        }

        public async Task SendPaymentSuccessNotification(string movieTitle, DateTime showtime, string ticketInfo, string userToken)
        {
            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = "Payment Successful!",
                    Body = $"Your booking for {movieTitle} at {showtime:g} is confirmed."
                },
                Data = new Dictionary<string, string>()
                {
                    { "type", "payment_success" },
                    { "ticketInfo", ticketInfo },
                    { "movieTitle", movieTitle },
                    { "showtime", showtime.ToString("o") }
                },
                Token = userToken
            };

            var response = await _messaging.SendAsync(message);
            Console.WriteLine($"Successfully sent message: {response}");
        }
    }

    // Extension methods for registering the service
    public static class NotificationServiceExtensions
    {
        public static IServiceCollection AddFirebaseNotifications(this IServiceCollection services, IConfiguration configuration)
        {
            var firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(configuration["Firebase:CredentialPath"])
            });

            services.AddSingleton<INotificationService>(provider => new FCMNotificationService(firebaseApp));

            return services;
        }
    }
}
