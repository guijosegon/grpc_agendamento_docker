using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ScheduleService.Services.Models;

namespace booking.schedule.Services.Repositories
{
    public interface IScheduleRepository
    {
        Task<Reservation> CreateAsync(DateTime start, DateTime end, long userId, string description);
        Task<bool> CancelAsync(long reservationId);
        Task<List<Reservation>> ListByUserAsync(long userId);
    }

    public class FileScheduleRepository : IScheduleRepository
    {
        private const string FilePath = "/data/reservations.json";
        private readonly List<Reservation> _reservations;
        private long _nextId;

        public FileScheduleRepository()
        {
            if (File.Exists(FilePath))
                _reservations = JsonSerializer.Deserialize<List<Reservation>>(File.ReadAllText(FilePath)) ?? new List<Reservation>();
            else
                _reservations = new List<Reservation>();

            _nextId = _reservations.Any() ? _reservations.Max(r => r.ReservationId) + 1 : 1;
        }

        public async Task<Reservation> CreateAsync(DateTime start, DateTime end, long userId, string description)
        {
            bool conflict = _reservations.Any(r => r.Status == "CONFIRMADO" && !(end <= r.StartTime || start >= r.EndTime));
            if (conflict) return null;

            var reservation = new Reservation
            {
                ReservationId = _nextId++,
                StartTime = start,
                EndTime = end,
                UserId = userId,
                Description = description,
                Status = "CONFIRMADO"
            };

            _reservations.Add(reservation);
            Persist();

            return reservation;
        }

        public async Task<bool> CancelAsync(long reservationId)
        {
            var res = _reservations.FirstOrDefault(r => r.ReservationId == reservationId);
            if (res is null) return false;

            res.Status = "CANCELADO";
            Persist();

            return true;
        }

        public async Task<List<Reservation>> ListByUserAsync(long userId)
        {
            var list = _reservations.Where(r => r.UserId == userId).OrderBy(r => r.StartTime).ToList();

            return list;
        }

        private void Persist()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_reservations, options);
            File.WriteAllText(FilePath, json);
        }
    }
}