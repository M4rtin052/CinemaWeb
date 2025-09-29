namespace Cinema.Models
{
    public class CalendarTime
    {
        public List<DateTime> DateTime { get; set; }
        public ICollection<ShowingModel> ShowingModel { get; set; }
    }
}
