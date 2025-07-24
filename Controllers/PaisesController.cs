using AgenciaDeToursRD.Data;
using AgenciaDeToursRD.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace AgenciaDeToursRD.Controllers
{
    public class PaisesController : Controller


    {
        private readonly AgenciaDeToursDbContext _context;

        public PaisesController( AgenciaDeToursDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var paises = _context.Paises
                .Include(p => p.Destinos)
                .ToList();

            return View(paises);
        }


        // GET: DestinosController/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var pais = _context.Paises
                .Include(p => p.Destinos)
                .FirstOrDefault(p => p.ID == id);

            if (pais == null)
                return NotFound();

            return View(pais);
        }

        // GET: DestinosController/Create
        public ActionResult Create()
        {
            var pais = new Pais
            {
                Destinos = new List<Destino>() 
            };
            return View(pais);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pais pais)
        {
            if (pais.Destinos == null || !pais.Destinos.Any(d => !string.IsNullOrWhiteSpace(d.Nombre)))
            {
                ModelState.AddModelError("Destinos", "Debes agregar al menos un destino válido.");
            }

            if (!ModelState.IsValid)
            {
                if (pais.Destinos == null)
                    pais.Destinos = new List<Destino>();
                return View(pais);
            }

            try
            {
                var newPais = new Pais
                {
                    Nombre = pais.Nombre,
                    Destinos = new List<Destino>()
                };

                foreach (var destino in pais.Destinos)
                {
                    if (string.IsNullOrWhiteSpace(destino.Nombre))
                        continue;

                    if (_context.Destinos.Any(d => d.Nombre == destino.Nombre))
                    {
                        ModelState.AddModelError("Destinos", $"El destino '{destino.Nombre}' ya existe en la base de datos.");
                        return View(pais);
                    }

                    if (newPais.Destinos.Any(d => d.Nombre == destino.Nombre))
                    {
                        ModelState.AddModelError("Destinos", $"El destino '{destino.Nombre}' está repetido.");
                        return View(pais);
                    }

                    newPais.Destinos.Add(new Destino
                    {
                        Nombre = destino.Nombre,
                        DuracionTexto = destino.DuracionTexto
                    });
                }

                _context.Paises.Add(newPais);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado: {ex.Message}");
                if (pais.Destinos == null)
                    pais.Destinos = new List<Destino>();
                return View(pais);
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (id <= 0)
                ModelState.AddModelError(string.Empty, "Id invalido");

            var pais = _context.Paises
                .Include(p => p.Destinos)
                .FirstOrDefault(p => p.ID == id);

            if (pais == null)
                ModelState.AddModelError(string.Empty, $"No existe pais con el id {id}");

            return View(pais);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Pais paisRequest)
        {
            try
            {
                var pais = _context.Paises
               .Include(p => p.Destinos)
               .FirstOrDefault(p => p.ID == id);

                if (pais == null)
                    ModelState.AddModelError(string.Empty, $"No existe pais con el id {id}");


                if (_context.Paises.Any(p => p.Nombre.Equals(paisRequest.Nombre) && p.ID != paisRequest.ID))
                    ModelState.AddModelError(string.Empty, $"El pais {paisRequest.Nombre} ya existe");
                

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DestinosController/Delete/5
        public ActionResult Delete(int id)
        {
            var pais = _context.Paises
       .Include(p => p.Destinos)
       .FirstOrDefault(p => p.ID == id);

            if (pais == null)
                return NotFound();

            return View(pais);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var pais = _context.Paises
                .Include(p => p.Destinos)
                .FirstOrDefault(p => p.ID == id);

            if (pais == null)
                return NotFound();

   
            if (pais.Destinos != null && pais.Destinos.Any())
                _context.Destinos.RemoveRange(pais.Destinos);

            _context.Paises.Remove(pais);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}
