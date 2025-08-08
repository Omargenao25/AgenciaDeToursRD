using AgenciaDeToursRD.Data;
using AgenciaDeToursRD.Models;
using ClosedXML.Excel;
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


        public FileResult ExportToursToCsv()
        {
            var tours = _context.Tours
                .Include(t => t.Destino)
                .Include(t => t.Destino.Pais)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Tours");

           
                worksheet.Cell(1, 1).Value = "Nombre";
                worksheet.Cell(1, 2).Value = "País";
                worksheet.Cell(1, 3).Value = "Destino";
                worksheet.Cell(1, 4).Value = "Fecha";
                worksheet.Cell(1, 5).Value = "Hora";
                worksheet.Cell(1, 6).Value = "Duración";
                worksheet.Cell(1, 7).Value = "Fecha Fin";
                worksheet.Cell(1, 8).Value = "Estado";
                worksheet.Cell(1, 9).Value = "Precio";
                worksheet.Cell(1, 10).Value = "ITBIS";

                
                for (int i = 0; i < tours.Count; i++)
                {
                    var t = tours[i];
                    worksheet.Cell(i + 2, 1).Value = t.Nombre;
                    worksheet.Cell(i + 2, 2).Value = t.Destino?.Pais?.Nombre;
                    worksheet.Cell(i + 2, 3).Value = t.Destino?.Nombre;
                    worksheet.Cell(i + 2, 4).Value = t.Fecha.ToString("dd/MM/yyyy");
                    worksheet.Cell(i + 2, 5).Value = t.Hora.ToString(@"hh\:mm");
                    worksheet.Cell(i + 2, 6).Value = t.Duracion;
                    worksheet.Cell(i + 2, 7).Value = t.FechaFin.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(i + 2, 8).Value = t.Estado;
                    worksheet.Cell(i + 2, 9).Value = t.Precio;
                    worksheet.Cell(i + 2, 10).Value = t.ITBIS;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Tours.xlsx");
                }
            }
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
            ViewBag.NombreDestino = "";
            ViewBag.DuracionDestino = "";
            ViewBag.ITBIS = "";
            ViewBag.FechaFin = "";
            ViewBag.Estado = "";
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Tour tour)
        {
            if (tour.DestinoID <= 0)
            {
                ModelState.AddModelError("DestinoID", "Debes seleccionar un destino.");
            }

            if (_context.Tours.Any(t => t.Nombre == tour.Nombre))
            {
                ModelState.AddModelError("Nombre", "Ya existe un tour con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                var destino = _context.Destinos
                    .Include(d => d.Pais)
                    .FirstOrDefault(d => d.ID == tour.DestinoID);

                ViewBag.Paises = new SelectList(_context.Paises, "ID", "Nombre", tour.PaisID);
                ViewBag.NombreDestino = destino?.Nombre ?? "";
                ViewBag.DuracionDestino = destino?.DuracionTexto ?? "";
                ViewBag.ITBIS = tour.ITBIS.ToString("0.00");
                ViewBag.FechaFin = tour.FechaFin.ToString("dd/MM/yyyy HH:mm");
                ViewBag.Estado = tour.Estado;

                return View(tour);
            }

            _context.Tours.Add(tour);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }




        [HttpGet]
        public IActionResult Edit(int id)
        {
            var tour = _context.Tours
                .Include(t => t.Destino)
                .ThenInclude(d => d.Pais)
                .FirstOrDefault(t => t.ID == id);

            if (tour == null)
                return NotFound();

            ViewBag.NombrePais = tour.Destino?.Pais?.Nombre;
            ViewBag.NombreDestino = tour.Destino?.Nombre ?? "";
            ViewBag.DuracionDestino = tour.Destino?.DuracionTexto ?? "";
            ViewBag.ITBIS = tour.ITBIS.ToString("0.00");
            ViewBag.FechaFin = tour.FechaFin.ToString("dd/MM/yyyy HH:mm");
            ViewBag.Estado = tour.Estado;

            return View(tour);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Tour tour)
        {
            if (id != tour.ID)
            {
                ViewBag.Error = "ID del tour no coincide.";
                return View(tour);
            }

            if (_context.Tours.Any(t => t.Nombre == tour.Nombre && t.ID != tour.ID))
            {
                ViewBag.Error = "Ya existe otro tour con ese nombre.";
                return View(tour);
            }

            if (tour.DestinoID <= 0)
            {
                ViewBag.Error = "Debes seleccionar un destino.";
                return View(tour);
            }

            try
            {
                _context.Tours.Update(tour);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al guardar cambios: {ex.Message}";
                return View(tour);
            }
        }




        [HttpGet]
        public ActionResult Delete(int id)
        {
            var tour = _context.Tours
                .Include(t => t.Pais)
                .Include(t => t.Destino)
                .FirstOrDefault(t => t.ID == id);

            if (tour == null)
            {
                TempData["ErrorMessage"] = "No se encontró el tour para eliminar.";
                return RedirectToAction("Index");
            }

            return View(tour);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var tour = _context.Tours.Find(id);

            if (tour == null)
            {
                TempData["ErrorMessage"] = "El tour ya no existe.";
                return RedirectToAction("Index");
            }

            try
            {
                _context.Tours.Remove(tour);
                _context.SaveChanges();
                return RedirectToAction("Details", "Paises", new { id = tour.PaisID });

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el tour: {ex.Message}";
                return RedirectToAction("Index");
            }
        }




        public JsonResult ObtenerDestino(int? id)
        {
            if (id == null)
                return Json(new { success = false, message = "ID de país no proporcionado" });

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
                return Json(new { success = false, message = "No se encontraron destinos para el país seleccionado" });

            Random aleatorio = new Random();
            var destinoAleatorio = destinos[aleatorio.Next(destinos.Count)];

            var tourAsociado = _context.Tours
                .Where(t => t.DestinoID == destinoAleatorio.ID)
                .OrderBy(t => t.ID)
                .FirstOrDefault();

            decimal precio = tourAsociado?.Precio ?? 0;

            return Json(new
            {
                success = true,
                idDestino = destinoAleatorio.ID,
                nombre = destinoAleatorio.Nombre,
                duracion = destinoAleatorio.DuracionTexto,
                precio = precio
            });
        }
    }
}