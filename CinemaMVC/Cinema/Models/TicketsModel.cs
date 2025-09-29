using Cinema.Models;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace Cinema.Models
{
    public class TicketsModel
    {
        [Key]
        public long Ticket_Id { get; set; }
        public int User_Id { get; set; }
        public int Seats_Id { get; set; }
        public int Showing_Id { get; set; }
        public float Price { get; set; }
        public virtual SeatsModel Seats { get; set; } = null!;
        public virtual ShowingModel Showing { get; set; } = null!;
        public virtual LoggingModel Logging { get; set; } = null!;
    }
}
