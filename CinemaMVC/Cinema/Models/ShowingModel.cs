using Cinema.Models;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class ShowingModel
    {
        [Key]
        public int Showing_Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Wartość musi być większa lub równa {1}.")]
        public int CinemaHall_Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Wartość musi być większa lub równa {1}.")]
        public int Film_Id { get; set; }
        public DateTime DateTime { get; set; }

        public virtual CinemaHallModel CinemaHall { get; set; } = null!;
        public virtual FilmInformationModel Films { get; set; } = null!;
        public virtual ICollection<TicketsModel> Tickets { get; set; } = new List<TicketsModel>();
    }
}
