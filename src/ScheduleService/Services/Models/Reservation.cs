using System;

namespace ScheduleService.Services.Models
{
    public class Reservation
    {
        public long ReservationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long UserId { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}