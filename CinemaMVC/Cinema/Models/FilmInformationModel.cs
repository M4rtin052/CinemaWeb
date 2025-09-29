using System.Drawing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // biblioteka, która pozwala na ręczne zdefiniowanie PK (primary key)

namespace Cinema.Models
{
    public class FilmInformationModel
    {
        [Key]
        public int Film_Id { get; set; }
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Age_category { get; set; } = null!;
        public string Duration { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Cast { get; set; } = null!;
        public string Director { get; set; } = null!;
        public string Production { get; set; } = null!;
        public DateOnly Release_date { get; set; }
        public string? MainPicture { get; set; }
        public string? BackgroundPicture { get; set; }
        public virtual ICollection<ShowingModel> Showing { get; set; } = new List<ShowingModel>();
    }
}
