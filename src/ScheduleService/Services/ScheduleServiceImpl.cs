using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using booking.schedule.Services.Repositories;
using Booking.Notify;
using Google.Protobuf;
using Grpc.Core;

namespace Booking.Schedule.Services
{
    public class ScheduleServiceImpl : ScheduleService.ScheduleServiceBase
    {
        private readonly IScheduleRepository _repo;
        private readonly NotificationService.NotificationServiceClient _notifClient;

        public ScheduleServiceImpl(IScheduleRepository repo, NotificationService.NotificationServiceClient notifClient)
        {
            _repo = repo;
            _notifClient = notifClient;
        }

        public override async Task<ReservationResponse> CreateReservation(CreateReservationRequest request, ServerCallContext context)
        {
            var start = DateTime.Parse(request.StartTime);
            var end = DateTime.Parse(request.EndTime);

            var res = await _repo.CreateAsync(start, end, request.UserId, request.Description) ?? throw new RpcException(new Status(StatusCode.AlreadyExists, "Já existe outro agendamento nesse horário"));

            try
            {
                var notification = new Notification
                {
                    UserId = res.UserId,
                    Message = $"Reserva #{res.ReservationId} confirmada para {res.StartTime.GetDateTimeFormats()} – {res.EndTime.GetDateTimeFormats()} com a descrição de {res.Description}"
                };

                _notifClient.SendNotification(notification);
            }
            catch
            {
                throw new RpcException(new Status(StatusCode.Internal, "Erro ao enviar notificacao, mas agendamento realizado!"));
            }

            return new ReservationResponse
            {
                ReservationId = res.ReservationId,
                StartTime = res.StartTime.ToString(),
                EndTime = res.EndTime.ToString(),
                UserId = res.UserId,
                Status = res.Status,
                Description = res.Description
            };
        }

        public override async Task<CancelResponse> CancelReservation(CancelRequest request, ServerCallContext context)
        {
            bool ok = await _repo.CancelAsync(request.ReservationId);
            return new CancelResponse { Success = ok };
        }

        public override async Task<ListReservationsResponse> ListReservations(ListReservationsRequest request, ServerCallContext context)
        {
            var list = await _repo.ListByUserAsync(request.UserId);
            var response = new ListReservationsResponse();

            response.Reservations.AddRange(list.Select(r => new ReservationResponse
            {
                ReservationId = r.ReservationId,
                StartTime = r.StartTime.ToString("o"),
                EndTime = r.EndTime.ToString("o"),
                UserId = r.UserId,
                Status = r.Status,
                Description = r.Description
            }));
            return response;
        }

        public override async Task<UploadAck> UploadAttachment(IAsyncStreamReader<FileChunk> requestStream, ServerCallContext context)
        {
            if (!await requestStream.MoveNext())
                return new UploadAck { Success = false, Message = "Nenhum chunk enviado." };

            var first = requestStream.Current;
            var filename = Path.GetFileName(first.Filename);
            var path = Path.Combine("/data", filename);
            Directory.CreateDirectory("/data");

            await File.WriteAllBytesAsync(path, first.Data.ToByteArray());

            while (await requestStream.MoveNext())
            {
                var chunk = requestStream.Current.Data.ToByteArray();
                await using var stream = new FileStream(path, FileMode.Append, FileAccess.Write);
                await stream.WriteAsync(chunk, 0, chunk.Length);
            }

            return new UploadAck { Success = true, Message = $"Arquivo '{filename}' salvo." };
        }

        public override async Task DownloadAttachment(AttachmentRequest request, IServerStreamWriter<FileChunk> responseStream, ServerCallContext context)
        {
            var filename = Path.GetFileName(request.Filename);
            var path = Path.Combine("/data", filename);

            if (!File.Exists(path))
                throw new RpcException(new Status(StatusCode.NotFound, $"Arquivo '{filename}' não encontrado"));

            const int chunkSize = 64 * 1024;
            byte[] buffer = new byte[chunkSize];
            bool first = true;

            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            int bytesRead;
            while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await responseStream.WriteAsync(new FileChunk
                {
                    Filename = first ? filename : string.Empty,
                    Data = ByteString.CopyFrom(buffer, 0, bytesRead)
                });
                first = false;
            }
        }
    }
}