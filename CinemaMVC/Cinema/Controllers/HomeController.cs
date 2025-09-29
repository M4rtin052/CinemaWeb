using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Cinema.Models;
using Cinema.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using NuGet.Configuration;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Cinema.Controllers;

public class HomeController : Controller
{
    private readonly MyDbContext _context;

    public HomeController(MyDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Films()
    {
        var films = _context.Films.ToList();

        return View(films);
    }
    public IActionResult UserPanel()
    {
        // sprawdzenie czy u�ytkownik jest zalogowany
        if (User.Identity.IsAuthenticated)
        {
            // pobranie identyfikatora to�samo�ci u�ytkownika
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // srpawdzenie czy zmienna z identyfikatorem nie jest pusta
            if (!userID.IsNullOrEmpty())
            {
                // pobranie z bazy danych bilet�w danego u�ytkownika
                // oraz encji zawieraj�cych sal� kina i opis film�w
                var tickets = _context.Tickets.Where(x => x.User_Id == int.Parse(userID))
                    .Include(x => x.Seats.CinemaHall).Include(x => x.Showing.Films).ToList();
                // zwr�cenie do widoku listy modelu bilet�w
                return View(tickets);
            }
            // przekierowanie do widoku login w kontrolerze login
            return RedirectToAction("Login", "Login");
        }
        // przekierowanie do widoku login w kontrolerze login
        return RedirectToAction("Login", "Login");
    }

    [HttpPost]
    public async Task<IActionResult> SaveTicket(List<int> selectedSeats, DateTime selectedDate, List<int> discount)
    {
        // pobranie identyfikatora to�samo�ci u�ytkownika
        var userID = User.FindFirstValue(ClaimTypes.NameIdentifier); // zwraca string
        // pobranie z bazy danych pasuj�cych informacji
        var seatsID = _context.Seats.Where(x => selectedSeats.Contains(x.Seats_Id)).ToList();
        var showingID = _context.Showing.Where(x => x.DateTime == selectedDate).FirstOrDefault();
        float price = 25f;
        // sprawdzanie warunku, czy �adna ze zmiennych nie jest pusta
        if (!userID.IsNullOrEmpty() && seatsID != null && showingID != null)
        {
            // p�tla tworz�ca nowy bilet dla ka�dego siedzenia
            for(int i = 0; i < seatsID.Count(); i++)
            {
                // warunek sprawdzaj�cy, czy lista parametru zawiera konkretne Id siedzenia
                if (discount.Contains(seatsID[i].Seats_Id))
                {
                    price = 20f;
                }
                else
                {
                    price = 25f;
                }
                // utworzenie nowego modelu i przypisanie go do zmiennej
                var addTicket = new TicketsModel
                {
                    User_Id = int.Parse(userID), // konwertowanie string na int
                    Seats_Id = seatsID[i].Seats_Id,
                    Showing_Id = showingID.Showing_Id,
                    Price = price
                };
                // dodanie i zapisanie do bazy danych modelu bilet�w
                _context.Add(addTicket);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("UserPanel");
        }

        return RedirectToAction("Summary", "Showing");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

[ApiController]
[Route("api/[controller]")]
public class DataReceiveController : ControllerBase
{
    // wstrzykiwanie zale�no�ci kontekstu bazy danych
    private readonly MyDbContext _context;
    public DataReceiveController(MyDbContext context)
    {
        _context = context;
    }
    [HttpPost]
    public async Task<IActionResult> DataReceive([FromBody] ArduinoDataModel model)
    {
        if (model == null) // sprawdzenie czy model zawiera dane
        {
            return BadRequest("Brak danych!"); // zwr�cenie odpowiedzi przez protok� HTTP
        }
        // pobranie do zmiennej listy sal kinowych
        var hallToUpdate = _context.CinemaHall.ToList();

        // p�tla do aktualizacji rekord�w w tabeli z salami
        foreach (var item in hallToUpdate)
        {
            item.Temperature = Math.Round(model.Temperature, 2); // zaokr�glenie odebranej temperatury do dw�ch miejsc po przecinku
            item.Lightening = model.Lightening; // zapisanie dla odpowiedniej sali, jej o�wietlenia
            _context.Update(item); // aktualizowanie bazy danych
            await _context.SaveChangesAsync(); // zapisanie zmian
        }

        return Ok("Dane odebrane."); // zwr�cenie odpowiedzi przez protok� HTTP

    }
}