using Cinema.Database;
using Cinema.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
using System.Security.Principal;

namespace Cinema.Controllers
{
    [Authorize]
    public class LoginController : Controller
    {
        private readonly MyDbContext _context;

        public LoginController(MyDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View("Views/Account/Login.cshtml");
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(LoggingModel model)
        {
            ModelState.Remove("Email");
            if (ModelState.IsValid) 
            {
                var user = _context.Logging.Where(x => x.Login == model.Login.Trim()).ToList(); // .Trim() - usuwa białe znaki z początku i końca stringa

                if (user.Count() != 0)
                {
                    var hasher = new PasswordHasher<LoggingModel>();
                    var resultHashedPassword = user.FirstOrDefault(x => hasher.VerifyHashedPassword(x, x.Password, model.Password) == PasswordVerificationResult.Success);

                    if (resultHashedPassword != null)
                    {
                        resultHashedPassword.LastLogin = DateTime.Now;
                        _context.SaveChanges();

                        var claims = new List<Claim> // tworzenie nowej instancji klasy "Claim", zawiera część stałych informacji o użytkowniku
                        {
                            new Claim(ClaimTypes.Name, resultHashedPassword.Login), // nowe roszczenie o typie "ClaimTypes.Name" o wartości "resultHashedPassword.Login"
                            new Claim(ClaimTypes.NameIdentifier, resultHashedPassword.User_Id.ToString())
                        };

                        if (resultHashedPassword.IsAdmin == true) // sprawdzenie czy użytkownik jest adminem
                        {
                            claims.Add(new Claim(ClaimTypes.Role, "Administrator")); // dodanie do listy nowej klasy "Claim", konkretniej roli o nazwie "Administrator"
                        }

                        // tworzenie nowej instancji klasy "ClaimsIdentity", zawierającej zbiór informacji o użytkowniku ze zmiennej "claims"
                        // jako schemat uwierzytelniania używane są ciasteczka
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); 
                        // tworzenie nowej instancji klasy "ClaimsPrincipal", reprezentacji logowanego użytkownika
                        var principal = new ClaimsPrincipal(claimsIdentity);

                        // asynchroniczne logowanie do aplikacji
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        return RedirectToAction("Index", "Home");
                    }

                }
            }

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View("Views/Account/Register.cshtml");
        }

        [AllowAnonymous] // klasa pozwalająca niezalogowanym użytkownikom z niej korzystać
        [ValidateAntiForgeryToken] // klasa sprawdzająca poprawność odebranych danych
        [HttpPost] // klasa służąca do przesyłania danych do serwera
        public async Task<IActionResult> Register([Bind("Login, Password, Email")] LoggingModel model) // atrybut Bind
        {
            if (ModelState.IsValid) // warunek sprawdzający poprawność danych
            {
                var hasher = new PasswordHasher<LoggingModel>(); // tworzenie nowej instancji klasy przekazanego modelu z widoku
                var users = from x in _context.Logging select x; // pobranie wszystkich użytkowników z bazy danych

                // sprawdzanie zaszyfrowanych haseł wszystkich użytkowników, czy jakikolwiek się powiela z wpisanym hasłem
                // i zapisanie listy wyników do zmiennej
                var checkPasswordExistance = users.Select(x => hasher.VerifyHashedPassword(x, x.Password, model.Password) == PasswordVerificationResult.Success).ToList();

                // warunek sprawdzający, czy jakiekolwiek hasło się powtarza
                if (checkPasswordExistance.Any(x => x == true))
                {
                    return RedirectToAction("Register"); // powrót do akcji strony rejestracji
                }
                else
                {
                    model.IsAdmin = false; // ustawienie, że użytkownik nie jest administratorem

                    model.Password = hasher.HashPassword(model, model.Password); // hashowanie hasła (zabezpieczenie)

                    await _context.AddAsync(model); // asynchroniczne dodanie danych parametru model do kontekstu bazy danych

                    await _context.SaveChangesAsync(); // asynchroniczne zapisanie wprowadzonych zmian
                    return RedirectToAction("Login"); // powrót do akcji strony logowania
                }
                
            }

            return RedirectToAction("Register");
        }
    }
}
