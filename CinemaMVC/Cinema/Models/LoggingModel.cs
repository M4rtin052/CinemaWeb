using Cinema.Models;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class LoggingModel
    {
        [Key]
        public int User_Id { get; set; }
        [Required]
        [StringLength(20, ErrorMessage = "Length of {0} must be between {2} and {1}.", MinimumLength = 4)]
        public string Login { get; set; } = null!;
        [Required]
        [StringLength(100, ErrorMessage = "Length of {0} must be between {2} and {1}.", MinimumLength = 4)]
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool? IsAdmin { get; set; }
        public DateTime? LastLogin { get; set; }

        public virtual ICollection<TicketsModel> Tickets { get; set; } = new List<TicketsModel>();
    }
}
