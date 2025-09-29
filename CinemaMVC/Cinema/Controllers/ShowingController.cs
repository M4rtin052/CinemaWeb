using Cinema.Database;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Mono.TextTemplating;
using System.Linq;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace Cinema.Controllers
{
    public class ShowingController : Controller
    {
        private readonly MyDbContext _context;

        public ShowingController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult ShowTime(int id, DateTime move)
        {
            if (User.Identity.IsAuthenticated)
            {
                var properFilm = _context.Showing.Where(x => x.Film_Id == id).Include(x => x.Films).ToList();
                var date = properFilm.Where(x => x.DateTime.Date == move.Date).ToList();

                return View(date);
            }
            
            return RedirectToAction("Login", "Login");
        }

        public IActionResult Seats(int hallId, DateTime playTime)
        {
            // warunek sprawdzający, czy użytkownik jest dalej zalogowany
            if (User.Identity.IsAuthenticated)
            {
                // zapisanie do zmiennej, listy zawierającej model miejsc na konkretnej sali
                // z uwzględnieniem modelu seansów o odpowiedniej dacie
                // oraz uwzględnieniem modelu biletów (pozwoli na sprawdzanie zajętości miejsc)
                var seatsInHall = _context.Seats.Where(x => x.CinemaHall_Id == hallId)
                    .Include(x => x.CinemaHall.Showing.Where(x => x.DateTime == playTime))
                    .ThenInclude(x => x.Tickets).ToList();
                
                return View(seatsInHall);
            }

            return RedirectToAction("Login", "Login");
        }

        public IActionResult Summary()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }

            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public IActionResult Summary(List<int> selectedSeats, int hallId, DateTime playTime)
        {
            // warunek sprawdzający, czy użytkownik jest dalej zalogowany
            if (User.Identity.IsAuthenticated)
            {
                // warunek sprawdzający, czy wybrano jakieś miejsce
                if (selectedSeats.IsNullOrEmpty())
                {
                    // powrót do akcji widoku miejsc na sali
                    return RedirectToAction("Seats", new { hallId, playTime });
                }

                // zapisanie do zmiennej, listy zawierającej model miejsc o konkretnym identyfikatorze miejsca
                // z uwzględnieniem modelu seansów o odpowiedniej dacie
                // oraz uwzględnieniem modelu z informacjami o filmie
                var allInfoTicket = _context.Seats.Where(x => selectedSeats.Contains(x.Seats_Id))
                    .Include(x => x.CinemaHall.Showing.Where(x => x.DateTime == playTime))
                    .ThenInclude(x => x.Films).ToList();

                // zwracamy widok podsumowania z listą modelu zapisanego w zmiennej lokalnej
                return View("Summary", allInfoTicket);
            }

            // przekierowanie do strony logowania
            return RedirectToAction("Login", "Login");
        }
    }
}
