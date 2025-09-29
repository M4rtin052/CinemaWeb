using Cinema.Database;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Cinema.Controllers
{
    public class FilmInformationController : Controller
    {
        private readonly MyDbContext _context;

        public FilmInformationController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult FilmDescription(int id)
        {
            var films = _context.Films.Where(x => x.Film_Id == id).SingleOrDefault();

            // zapisanie do sesji wartości "id" pod nazwą "SelectedFilmId" -
            // wykorzystane przy finalizacji kupna biletu
            HttpContext.Session.SetInt32("SelectedFilmId", id);

            // pobieranie ścieżki do pliku obrazu tła
            var fileName = Path.GetFileName(films.BackgroundPicture);

            // przechowywanie w obiektach ViewBag (trwa tylko przez jedno żądanie HTTP) obrazu tła oraz głównego
            ViewBag.PictureBG = Url.Content($"~/Pictures/{id}/{fileName}");
            ViewBag.PictureMain = films.MainPicture;

            return View(films);
        }
        
    }
}
