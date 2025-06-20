using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Booking.Notify.Services
{
    public class NotificationServiceImpl : NotificationService.NotificationServiceBase
    {
        private readonly ILogger<NotificationServiceImpl> _logger;
        public NotificationServiceImpl(ILogger<NotificationServiceImpl> logger) => _logger = logger;

        public override async Task<Ack> SendNotification(Notification request, ServerCallContext context)
        {
            _logger.LogInformation($"Notificação para usuário {request.UserId}: {request.Message}");
            return new Ack { Sent = true };
        }
    }
}