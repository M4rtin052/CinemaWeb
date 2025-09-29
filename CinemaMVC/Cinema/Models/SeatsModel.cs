using Cinema.Models;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class SeatsModel
    {
        [Key]
        public int Seats_Id { get; set; }
        public int CinemaHall_Id { get; set; }
        public int Row { get; set; }
        public int Seat { get; set; }

        public virtual CinemaHallModel CinemaHall { get; set; } = null!;
        public virtual ICollection<TicketsModel> Tickets { get; set; } = new List<TicketsModel>();
    }
}
