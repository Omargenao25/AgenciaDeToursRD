using AgenciaDeToursRD.Data;
using AgenciaDeToursRD.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AgenciaDeToursRD.Controllers
{
    public class ToursController : Controller
    {
        private readonly AgenciaDeToursDbContext _context;

        public ToursController(AgenciaDeToursDbContext context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            try
            {
                var tours = _context.Tours
                    .Include(t => t.Pais)
                    .Include(t => t.Destino)
                    .ToList();

                if (tours == null || !tours.Any())
                {
                    TempData["InfoMessage"] = "No hay tours registrados en el sistema.";
                }

                return View(tours);
            }
            catch (Exception ex)
            {
               
                TempData["ErrorMessage"] = "Ocurrió un error al cargar los tours.";
                return RedirectToAction("Error", "Home");
            }
        }


        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "ID inválido para buscar detalles del tour.";
                return RedirectToAction("Index");
            }

            var tour = _context.Tours
                .Include(t => t.Pais)
                .Include(t => t.Destino)
                .FirstOrDefault(t => t.ID == id.Value);

            if (tour == null)
            {
                TempData["ErrorMessage"] = "No se encontró el tour solicitado.";
                return RedirectToAction("Index");
            }

            return View(tour);
        }



        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.PaisID = new SelectList(_context.Paises, "ID", "Nombre");
            ViewBag.DestinoID = new SelectList(new List<SelectListItem>());
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Tour tour)
        {
            if (ModelState.IsValid)
            {
                _context.Tours.Add(tour);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

   
            ViewBag.PaisID = new SelectList(_context.Paises, "ID", "Nombre", tour.PaisID);
            ViewBag.DestinoID = new SelectList(new List<SelectListItem>());
            return View(tour);
        }



        // GET: ToursController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ToursController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ToursController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ToursController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }


        }

        public JsonResult ObtenerDestino(int? id)
        {
            if (id == null)
            {
                return Json(null);
            }

            var pais = _context.Paises
                .Include(t => t.Destinos)
                .FirstOrDefault(t => t.ID == id.Value);

            if (pais == null || pais.Destinos == null || !pais.Destinos.Any())
            {
                return Json(null);
            }

            var destinos = pais.Destinos.ToList();
            Random aleatorio = new Random();
            int indice = aleatorio.Next(destinos.Count);

            var destinoSeleccionado = destinos[indice];

            return Json(new
            {
                idDestino = destinoSeleccionado.ID,
                nombreDestino = destinoSeleccionado.Nombre
            });
        }

        public JsonResult GetDestinosPorPais(int paisId)
        {
            var destinos = _context.Destinos
                .Where(d => d.PaisId == paisId)
                .Select(d => new { d.ID, d.Nombre })
                .ToList();

            return Json(destinos);
        }

    }

}
