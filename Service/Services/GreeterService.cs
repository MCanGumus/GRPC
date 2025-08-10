using Grpc.Core;
using Service;

namespace Service.Services
{
    public class GreeterService : NotificationProto.NotificationProtoBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override async Task StreamNotifications(EmptyRequest request, IServerStreamWriter<NotificationResponse> responseStream, ServerCallContext context)
        {
            int i = 0;
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var message = new NotificationResponse { Message = "Message: " + i++ };
                await responseStream.WriteAsync(message);
                await Task.Delay(1000);
            }
        }

        public override async Task StreamContentNotifications(ContentRequest request, IServerStreamWriter<NotificationResponse> responseStream, ServerCallContext context)
        {
            int i = 0;
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var message = new NotificationResponse { Message = request.Title + ": " + i++ };
                await responseStream.WriteAsync(message);
                await Task.Delay(1000);
            }
        }
    }
}
