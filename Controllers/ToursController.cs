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

                if (!tours.Any())
                {
                    ViewBag.InfoMessage = "No hay tours registrados en el sistema.";
                }

                return View(tours);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al cargar los tours: {ex.Message}";
                return View(new List<Tour>());
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

                try
                {
                    _context.Tours.Add(tour);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {

                    ModelState.AddModelError(string.Empty, $"Error al crear {ex.Message}");

                    ViewBag.PaisID = new SelectList(_context.Paises, "ID", "Nombre", tour.PaisID);
                    ViewBag.DestinoID = new SelectList(new List<SelectListItem>());
                    return View(tour);
                }

            }


            public ActionResult Edit(int id)
            {
                return View();
            }


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


            public ActionResult Delete(int id)
            {
                return View();
            }


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
                    return Json(null);


                var destinos = _context.Destinos
                    .Where(d => d.PaisId == id.Value)
                    .Select(d => new
                    {
                        d.ID,
                        d.Nombre,
                        d.DuracionTexto
                    })
                    .ToList();

                if (!destinos.Any())
                    return Json(null);

                Random aleatorio = new Random();
                var destinoAleatorio = destinos[aleatorio.Next(destinos.Count)];

                return Json(new
                {
                    idDestino = destinoAleatorio.ID,
                    nombre = destinoAleatorio.Nombre,

                });
            }

        }
    }