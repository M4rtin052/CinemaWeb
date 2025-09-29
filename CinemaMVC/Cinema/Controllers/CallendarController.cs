using Cinema.Database;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Cinema.Controllers
{
    public class CallendarController : Controller
    {
        private readonly List<DateTime> _todaysDate = new List<DateTime> { new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) };
        
        private readonly MyDbContext _context;
        public CallendarController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Callendar(int id)
        {
            if(User.Identity.IsAuthenticated)
            {
                var model = new CalendarTime
                {
                    DateTime = null,
                    ShowingModel = null
                };

                // zapisanie do zmiennej listy dat i godzin, w której obecna jest aktualna data
                var allDays = _todaysDate;
                // zapisanie do zmiennej pobranych, pogrupowanych dat seansów o odpowiednim Id filmu
                var filmsToDate = _context.Showing.Where(x => x.Film_Id == id).OrderBy(x => x.DateTime).ToList();

                // zapisanie do zmiennych typu integer, pierwszych indeksów z listy, kolejno: roku, miesiąca, dnia
                int year = allDays[0].Year;
                int month = allDays[0].Month;
                int day = allDays[0].Day;
                
                // wyczyszczenie zmiennej z obecnej daty
                allDays.Clear();

                // pobranie do zmiennej ilości dni w danym miesiącu
                int daysInMonth = DateTime.DaysInMonth(year, month);

                // pętla do uzyskania każdego dnia miesiąca
                for (int days = 1; days <= daysInMonth; days++)
                {
                    allDays.Add(new DateTime(year, month, days));
                }

                if (TempData["changedMonth"] == null)
                {
                    model = new CalendarTime
                    {
                        DateTime = allDays,
                        ShowingModel = filmsToDate
                    };
                    TempData["newMonth"] = JsonConvert.SerializeObject(model);
                }
                else
                {
                    var modelJSON = TempData["changedMonth"] as string;
                    model = JsonConvert.DeserializeObject<CalendarTime>(modelJSON);

                    TempData["newMonth"] = JsonConvert.SerializeObject(model);
                }

                if (!model.ShowingModel.IsNullOrEmpty())
                {
                    return View("Views/Showing/Callendar.cshtml", model);
                }
                return BadRequest("Brak seansów.");
            }

            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeMonth(string move, int id, CalendarTime newMonth)
        {
            var modelJSON = TempData["newMonth"] as string;
            newMonth = JsonConvert.DeserializeObject<CalendarTime>(modelJSON);
            List<DateTime> changeMonth;

            if (newMonth.DateTime.FirstOrDefault().Month == DateTime.Now.Month)
            {
                changeMonth = _todaysDate;
            }
            else
            {
                changeMonth = newMonth.DateTime;
            }

            var filmsToDate = _context.Showing.Where(x => x.Film_Id == id).OrderBy(x => x.DateTime).ToList();

            DateTime changedDate;

            if (move == "left")
            {
                var moveLeft = changeMonth.LastOrDefault().AddMonths(-1);
                changedDate = moveLeft;

            }
            else
            {
                var moveRight = changeMonth.LastOrDefault().AddMonths(1);
                changedDate = moveRight;

            }

            changeMonth.Clear();

            if (changedDate.Month == DateTime.Now.Month && changedDate.Year == DateTime.Now.Year)
            {
                changeMonth.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
                return RedirectToAction("Callendar", new { id });
            }

            int changedYear = changedDate.Year;
            int changedMonth = changedDate.Month;

            int daysInMonth = DateTime.DaysInMonth(changedYear, changedMonth);

            for (int days = 1; days <= daysInMonth; days++)
            {
                changeMonth.Add(new DateTime(changedYear, changedMonth, days));
            }

            // utworzenie nowego modelu z potrzebnymi zmiennymi
            var model = new CalendarTime
            {
                DateTime = changeMonth,
                ShowingModel = filmsToDate
            };

            // zapisanie, do danych tymczasowych, modelu w formacie JSON
            TempData["changedMonth"] = JsonConvert.SerializeObject(model);
            // przejście do akcji kalendarz, z zapamiętaniem tego samego Id filmu
            return RedirectToAction("Callendar", new { id });
        }
    }
}
