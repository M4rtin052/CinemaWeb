using Cinema.Models;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class CinemaHallModel
    {
        [Key]
        public int CinemaHall_Id { get; set; }
        public string HallNumber { get; set; } = null!;
        public double? Temperature { get; set; }
        public string? Lightening { get; set; }

        public virtual ICollection<SeatsModel> Seats { get; set; } = new List<SeatsModel>();
        public virtual ICollection<ShowingModel> Showing { get; set; } = new List<ShowingModel>();
    }
}
