using Cinema.Database;
using Cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;

namespace Cinema.Controllers
{
    [Authorize(Roles ="Administrator")]
    public class AdminPanelController : Controller
    {
        private readonly MyDbContext _context;

        public AdminPanelController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Users()
        {
            // pobranie wszystkich rekordów z tabeli kont użytkowników i zapisanie ich jako lista do zmiennej
            var users = _context.Logging.Select(x => x).ToList();

            // zwrócenie widoku z listą modelu kont użytkowników
            return View("Views/AdminViews/Users/UserManage.cshtml", users);
        }

        public IActionResult CreateUser()
        {

            return View("Views/AdminViews/Users/CreateUser.cshtml");
        }

        public IActionResult UpdateUser(int id)
        {
            var userInfo = _context.Logging.Where(x => x.User_Id == id).SingleOrDefault();

            return View("Views/AdminViews/Users/UpdateUser.cshtml", userInfo);
        }

        public IActionResult DeleteUser(int id)
        {
            var userInfo = _context.Logging.Where(x => x.User_Id == id).SingleOrDefault();

            return View("Views/AdminViews/Users/DeleteUser.cshtml", userInfo);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(LoggingModel model, bool Admin)
        {
            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher<LoggingModel>();
                var users = from x in _context.Logging select x;

                var checkPasswordExistance = users.Select(x => hasher.VerifyHashedPassword(x, x.Password, model.Password) == PasswordVerificationResult.Success).ToList();

                if (checkPasswordExistance.Any(x => x == true))
                {
                    ModelState.AddModelError("", "Użytkownik z takim hasłem już istnieje!");
                    return View("Views/AdminViews/Users/CreateUser.cshtml");
                }
                else
                {
                    model.Password = hasher.HashPassword(model, model.Password);
                    if (Admin == true)
                    {
                        model.IsAdmin = true;
                    }
                    else
                    {
                        model.IsAdmin = false;
                    }

                    var createUser = new LoggingModel
                    {
                        Login = model.Login,
                        Password = model.Password,
                        Email = model.Email,
                        IsAdmin = model.IsAdmin
                    };

                    await _context.AddAsync(createUser);

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Users");
                }

            }

            return RedirectToAction("CreateUser");

        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(LoggingModel model, bool Admin)
        {
            var userToUpdate = await _context.Logging.FindAsync(model.User_Id);

            if (userToUpdate != null)
            {
                if (Admin == true)
                {
                    model.IsAdmin = true;
                }
                else
                {
                    model.IsAdmin = false;
                }

                userToUpdate.Login = model.Login.Trim();
                userToUpdate.Email = model.Email.Trim();
                userToUpdate.IsAdmin = model.IsAdmin;

                // aktualizowanie kontekstu bazy danych, przy pomocy zmiennej lokalnej, modelu "userToUpdate"
                // zmienna odnosi się tylko do wybranego użytkownika
                _context.Update(userToUpdate);
                // asynchroniczne zapisanie zmian w bazie danych
                await _context.SaveChangesAsync();

                return RedirectToAction("Users");
            }

            return RedirectToAction("UpdateUser");
            
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(LoggingModel model)
        {
            var userToDelete = await _context.Logging.FindAsync(model.User_Id);

            if (userToDelete != null)
            {
                // usunięcie z kontekstu bazy danych, przy pomocy zmiennej lokalnej, modelu "userToDelete"
                // zmienna odnosi się tylko do wybranego użytkownika
                _context.Remove(userToDelete);
                // asynchroniczne zapisanie zmian w bazie danych
                await _context.SaveChangesAsync();

                return RedirectToAction("Users");
            }

            return NotFound(model);
        }

        public IActionResult Films()
        {
            var films = _context.Films.Select(x => x).ToList();

            return View("Views/AdminViews/Films/FilmsManage.cshtml", films);
        }

        public IActionResult CreateFilm()
        {

            return View("Views/AdminViews/Films/CreateFilm.cshtml");
        }

        public IActionResult UpdateFilm(int id)
        {

            var filmInfo = _context.Films.Where(x => x.Film_Id == id).SingleOrDefault();

            return View("Views/AdminViews/Films/UpdateFilm.cshtml", filmInfo);
        }

        public IActionResult DeleteFilm(int id)
        {
            //var filmInfo = _context.Pictures.Where(x => x.Film_Id == id).Include(x => x.Films).SingleOrDefault();
            var filmInfo = _context.Films.Where(x => x.Film_Id == id).SingleOrDefault();

            return View("Views/AdminViews/Films/DeleteFilm.cshtml", filmInfo);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFilm(FilmInformationModel model)
        {
            if (model != null)
            {
                var createFilm = new FilmInformationModel
                {
                    Name = model.Name,
                    Category = model.Category,
                    Age_category = model.Age_category,
                    Duration = model.Duration,
                    Description = model.Description,
                    Cast = model.Cast,
                    Director = model.Director,
                    Production = model.Production,
                    Release_date = model.Release_date
                };
                
                await _context.AddAsync(createFilm);

                await _context.SaveChangesAsync();

                var getNewFilmId = _context.Films.Select(x => x.Film_Id).OrderBy(x => x).Last();

                createFilm.MainPicture = Path.Combine(getNewFilmId.ToString(), model.MainPicture);
                createFilm.BackgroundPicture = Path.Combine(getNewFilmId.ToString(), model.BackgroundPicture);

                string createNewFolder = Path.Combine("wwwroot", "Pictures", getNewFilmId.ToString());
                if (!Directory.Exists(createNewFolder))
                {
                    Directory.CreateDirectory(createNewFolder);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Films");
            }

            return RedirectToAction("CreateFilm");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFilm(FilmInformationModel model)
        {
            var filmToUpdate = await _context.Films.FindAsync(model.Film_Id);

            if (filmToUpdate != null)
            {
                if (filmToUpdate.MainPicture != null && filmToUpdate.BackgroundPicture != null)
                {
                    filmToUpdate.MainPicture = Path.Combine(model.Film_Id.ToString(), model.MainPicture);
                    filmToUpdate.BackgroundPicture = Path.Combine(model.Film_Id.ToString(), model.BackgroundPicture);

                    _context.Update(filmToUpdate);
                }
                
                filmToUpdate.Name = model.Name.Trim();
                filmToUpdate.Category = model.Category.Trim();
                filmToUpdate.Age_category = model.Age_category.Trim();
                filmToUpdate.Duration = model.Duration.Trim();
                filmToUpdate.Description = model.Description.Trim();
                filmToUpdate.Cast = model.Cast.Trim();
                filmToUpdate.Director = model.Director.Trim();
                filmToUpdate.Production = model.Production.Trim();
                filmToUpdate.Release_date = model.Release_date;
                
                
                _context.Update(filmToUpdate);

                await _context.SaveChangesAsync();

                return RedirectToAction("Films");
            }

            return RedirectToAction("UpdateFilm");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFilm(FilmInformationModel model)
        {
            var filmToDelete = await _context.Films.FindAsync(model.Film_Id);

            if (filmToDelete != null)
            {
                _context.Remove(filmToDelete);

                string deleteFolder = Path.Combine("wwwroot", "Pictures", filmToDelete.Film_Id.ToString());
                if (Directory.Exists(deleteFolder))
                {
                    Directory.Delete(deleteFolder, true);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Films");
            }

            return NotFound(model);

        }

        public IActionResult CinemaHall()
        {
            var halls = _context.CinemaHall.Select(x => x).Include(x => x.Showing).ToList();

            return View("Views/AdminViews/Halls/CinemaHallManage.cshtml", halls);
        }

        public IActionResult DeleteShow(int id)
        {
            var hallFilms = _context.Showing.Where(x => x.CinemaHall_Id == id).OrderBy(x => x.DateTime).Include(x => x.Films).ToList();

            return View("Views/AdminViews/Halls/DeleteShow.cshtml", hallFilms);
        }
        public IActionResult UpdateShow(int id)
        {
            var showInfo = _context.Showing.Where(x => x.Showing_Id == id).SingleOrDefault();

            return View("Views/AdminViews/Halls/UpdateShow.cshtml", showInfo);
        }
        public IActionResult CreateShow(int id)
        {
            var hallId = _context.Showing.Where(x => x.CinemaHall_Id == id).FirstOrDefault();

            return View("Views/AdminViews/Halls/CreateShow.cshtml", hallId);
        }

        [HttpPost]
        public async Task<IActionResult> CinemaHallDeleteShow(List<int> toDelete, int id)
        {
            var hallDeleteShows = _context.Showing.Where(x => toDelete.Contains(x.Showing_Id));

            if (!hallDeleteShows.IsNullOrEmpty())
            {
                _context.RemoveRange(hallDeleteShows);

                await _context.SaveChangesAsync();

                return RedirectToAction("DeleteShow", new { id });
            }

            TempData["Message"] = "Brak seansów do usunięcia.";
            return RedirectToAction("DeleteShow", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateShow(ShowingModel model, int id)
        {
            var showToUpdate = await _context.Showing.FindAsync(model.Showing_Id);
            var checkFilm = await _context.Films.FindAsync(model.Film_Id);
            var checkHall = await _context.CinemaHall.FindAsync(model.CinemaHall_Id);

            if (showToUpdate != null && checkFilm != null && checkHall != null)
            {
                showToUpdate.CinemaHall_Id = model.CinemaHall_Id;
                showToUpdate.Film_Id = model.Film_Id;
                showToUpdate.DateTime = model.DateTime;

                _context.Update(showToUpdate);

                await _context.SaveChangesAsync();
                return RedirectToAction("DeleteShow", new { id = showToUpdate.CinemaHall_Id });
            }

            if (checkFilm == null)
            {
                TempData["checkFilm"] = "Nie istnieje taki film. Sprawdź dostępne numery filmów w zakładce 'Filmy'.";
            }
            if (checkHall == null)
            {
                TempData["checkHall"] = "Nie istnieje taka sala. Sprawdź dostępne numery sal w zakładce 'Sale kinowe'.";
            }

            return RedirectToAction("UpdateShow");
        }
        [HttpPost]
        public async Task<IActionResult> CreateShow(ShowingModel model, int id)
        {
            var checkFilm = await _context.Films.FindAsync(model.Film_Id);
            var checkHall = await _context.CinemaHall.FindAsync(model.CinemaHall_Id);

            if (checkFilm != null && checkHall != null)
            {
                model.Films = _context.Films.Where(x => x.Film_Id == model.Film_Id).FirstOrDefault();
                model.CinemaHall = _context.CinemaHall.Where(x => x.CinemaHall_Id == model.CinemaHall_Id).SingleOrDefault();

                if (model.Films != null && model.CinemaHall != null)
                {
                    var createFilm = new ShowingModel
                    {
                        CinemaHall_Id = model.CinemaHall_Id,
                        Film_Id = model.Film_Id,
                        DateTime = model.DateTime,
                        Films = model.Films,
                        CinemaHall = model.CinemaHall
                    };

                    await _context.AddAsync(createFilm);

                    await _context.SaveChangesAsync();
                    return RedirectToAction("DeleteShow", new { id = createFilm.CinemaHall_Id });
                }

                return RedirectToAction("CreateShow");
            }

            if (checkFilm == null)
            {
                TempData["checkFilm"] = "Nie istnieje taki film. Sprawdź dostępne numery filmów w zakładce 'Filmy'.";
            }
            if (checkHall == null)
            {
                TempData["checkHall"] = "Nie istnieje taka sala. Sprawdź dostępne numery sal w zakładce 'Sale kinowe'.";
            }

            return RedirectToAction("CreateShow");
        }
        public IActionResult Tickets()
        {
            var tickets = _context.Tickets.Select(x => x).Include(x => x.Showing).ThenInclude(x => x.CinemaHall).Include(x => x.Logging).ToList();

            return View("Views/AdminViews/TicketsManage.cshtml", tickets);
        }

        [HttpPost]
        public async Task<IActionResult> Tickets(List<int> toDelete)
        {
            var deleteTickets = _context.Tickets.Where(x => toDelete.Contains((int)x.Ticket_Id));

            if (!deleteTickets.IsNullOrEmpty())
            {
                _context.RemoveRange(deleteTickets);

                await _context.SaveChangesAsync();

                return RedirectToAction("Tickets");
            }

            TempData["Message"] = "Brak biletów do usunięcia.";
            return RedirectToAction("Tickets");
        }
    }
}
