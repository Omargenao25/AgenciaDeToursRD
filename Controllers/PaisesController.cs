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
        // GET: Paises/Create
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
        public async Task<IActionResult> Create(Pais pais, IFormFile? BanderaFile)
        {
            if (!ModelState.IsValid)
            {
                return View(pais);
            }

            string? banderaUrl = null;

            if (BanderaFile != null && BanderaFile.Length > 0)
            {
                var extension = Path.GetExtension(BanderaFile.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(string.Empty, "Solo se permiten imágenes JPG o PNG.");
                    return View(pais);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await BanderaFile.CopyToAsync(stream);
                }

                banderaUrl = $"/uploads/{uniqueFileName}";
            }

            var newPais = new Pais
            {
                Nombre = pais.Nombre.Trim(),
                Bandera = banderaUrl,
                Destinos = new List<Destino>()
            };

            if (pais.Destinos != null)
            {
                foreach (var destino in pais.Destinos)
                {
                    if (!string.IsNullOrWhiteSpace(destino.Nombre))
                    {
                        newPais.Destinos.Add(new Destino
                        {
                            Nombre = destino.Nombre.Trim()
                        });
                    }
                }
            }

            try
            {
                _context.Paises.Add(newPais);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
                return View(pais);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pais = await _context.Paises
                .Include(p => p.Destinos)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (pais == null)
            {
                return NotFound();
            }

            return View(pais);
        }

        // POST: Paises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pais pais, IFormFile BanderaFile)
        {
            if (id != pais.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(pais);
            }

            try
            {
                var paisDb = await _context.Paises
                    .Include(p => p.Destinos)
                    .FirstOrDefaultAsync(p => p.ID == id);

                if (paisDb == null)
                {
                    return NotFound();
                }

                var existing = _context.Paises.Any(p => p.Nombre == pais.Nombre.ToUpper().Trim() && p.ID != id);

                if (existing)
                {
                    ModelState.AddModelError(string.Empty, "No se permiten duplicados");
                    return View(pais);
                }

                // Actualizar nombre
                paisDb.Nombre = pais.Nombre.ToUpper().Trim();

                // Procesar imagen si se envía
                if (BanderaFile != null && BanderaFile.Length > 0)
                {
                    var extension = Path.GetExtension(BanderaFile.FileName).ToLowerInvariant();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError(string.Empty, "Solo se permiten imágenes JPG o PNG.");
                        return View(pais);
                    }

                  
                    if (!string.IsNullOrEmpty(paisDb.Bandera))
                    {
                        var rutaAntigua = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", paisDb.Bandera.TrimStart('/'));
                        if (System.IO.File.Exists(rutaAntigua))
                        {
                            System.IO.File.Delete(rutaAntigua);
                        }
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await BanderaFile.CopyToAsync(stream);
                    }

                    paisDb.Bandera = $"/uploads/{uniqueFileName}";
                }

                // Actualizar destinos
                paisDb.Destinos.Clear();

                if (pais.Destinos != null)
                {
                    foreach (var destino in pais.Destinos)
                    {
                        if (string.IsNullOrWhiteSpace(destino.Nombre))
                        {
                            ModelState.AddModelError(string.Empty, "Todos los destinos deben tener nombre.");
                            return View(pais);
                        }

                        if (paisDb.Destinos.Any(d => d.Nombre == destino.Nombre.ToUpper().Trim()))
                        {
                            ModelState.AddModelError(string.Empty, $"Destino duplicado: {destino.Nombre}");
                            return View(pais);
                        }

                        paisDb.Destinos.Add(new Destino
                        {
                            Nombre = destino.Nombre.ToUpper().Trim(),
                            DuracionTexto = destino.DuracionTexto.ToUpper().Trim()
                        });
                    }
                }

                _context.Update(paisDb);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al editar: {ex.Message}");
                return View(pais);
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
