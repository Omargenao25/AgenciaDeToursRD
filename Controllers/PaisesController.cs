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
                Destinos = new List<Destino>() // <- importante
            };
            return View(pais);
        }

        // POST: DestinosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pais pais)
        {
            try
            {
                var newPais = new Pais
                {
                    Nombre = pais.Nombre.ToUpper(),
                    Destinos = new List<Destino>()
                };

                if (_context.Paises.Any(p => p.Nombre.Equals(pais.Nombre)))
                {
                    ModelState.AddModelError(string.Empty, $"El pais {pais.Nombre} ya existe");
                    return View(newPais);
                }

                foreach (var destino in pais.Destinos)
                {
                    var d  = _context.Destinos.FirstOrDefault(d => d.Nombre == destino.Nombre.ToUpper());

                    if (d != null)
                    {
                        ModelState.AddModelError("Destinos", $"El destino '{destino.Nombre}' ya existe en la base de datos.");
                        return View(pais);
                    }

                    var existing = newPais.Destinos.FirstOrDefault(d => d.Nombre == destino.Nombre.ToUpper());

                    if(existing != null)
                    {
                        ModelState.AddModelError("Destinos", $"El destino '{destino.Nombre}' está duplicado en la lista.");
                        return View(pais);
                    }

                    newPais.Destinos.Add(new Destino
                    {
                        Nombre = destino.Nombre.ToUpper(),
                        DuracionTexto = destino.DuracionTexto.ToUpper()
                    });
                }
                _context.Paises.Add(newPais);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Debes agregar al menos un destino al Pais");
               
            }
            return View(pais);
        }

        // GET: DestinosController/Edit/5
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

        // POST: DestinosController/Edit/5
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
            return View();
        }

        // POST: DestinosController/Delete/5
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
    }
}
