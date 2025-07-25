using AgenciaDeToursRD.Data;
using AgenciaDeToursRD.Excepciones;
using AgenciaDeToursRD.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace AgenciaDeToursRD.Controllers
{
    public class PaisesController : Controller


    {
        private readonly AgenciaDeToursDbContext _context;

        public PaisesController(AgenciaDeToursDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var paises = _context.Paises
                .Include(p => p.Destinos)
                .OrderBy(p => p.Nombre)
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

            if (pais != null)
            {
                pais.Destinos = pais.Destinos
                    .OrderBy(d => d.Nombre)
                    .ToList();
            }
            else
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Create(Pais pais, IFormFile BanderaFile)
        {
            try
            {
                if (pais.Destinos == null)
                {
                    pais.Destinos = new List<Destino>();
                }

                if (_context.Paises.Any(p => p.Nombre == pais.Nombre.ToUpperInvariant().Trim()))
                {
                    ModelState.AddModelError(string.Empty, "No se admiten paises duplicados");
                    return View(pais);
                }

                // Procesar imagen bandera (si hay archivo)
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

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await BanderaFile.CopyToAsync(stream);

                    banderaUrl = $"/uploads/{uniqueFileName}";
                }

                var destinosValidos = new List<Destino>();

                var nombresDestinos = pais.Destinos
                    .Where(d => !string.IsNullOrWhiteSpace(d.Nombre))
                    .Select(d => d?.Nombre?.ToUpper().Trim())
                    .ToList();

                var nombresExistentes = _context.Destinos
                    .Where(d => nombresDestinos.Contains(d.Nombre))
                    .Select(d => d.Nombre)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var destino in pais.Destinos)
                {
                    destino.Nombre = destino?.Nombre?.ToUpper().Trim();
                    destino.DuracionTexto = destino?.DuracionTexto?.ToUpper().Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(destino?.Nombre)) continue;

                    if (nombresExistentes.Contains(destino.Nombre))
                    {
                        ModelState.AddModelError(string.Empty, $"El destino '{destino.Nombre}' ya existe y no se admite duplicado.");
                        return View(pais);
                    }

                    if (destinosValidos.Any(d => d.Nombre == destino.Nombre))
                    {
                        ModelState.AddModelError(string.Empty, $"El destino '{destino.Nombre}' está repetido en el formulario.");
                        return View(pais);
                    }

                    destinosValidos.Add(destino);
                }

                var newPais = new Pais
                {
                    Nombre = pais.Nombre.ToUpper().Trim(),
                    Bandera = banderaUrl,
                    Destinos = destinosValidos
                };

                _context.Paises.Add(newPais);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al intentar guardar los datos");
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

        public async Task<IActionResult> Edit(int id, Pais pais, IFormFile BanderaFile)
        {
            try
            {
                var paisExistente = await _context.Paises
                    .Include(p => p.Destinos)
                    .FirstOrDefaultAsync(p => p.ID == id);

                if (paisExistente == null)
                    return NotFound();

                // Validar y procesar archivo de bandera si se subió uno
                if (BanderaFile != null && BanderaFile.Length > 0)
                {
                    var banderaError = await ProcesarArchivoBandera(paisExistente, BanderaFile);
                    if (!string.IsNullOrEmpty(banderaError))
                    {
                        ModelState.AddModelError(string.Empty, banderaError);
                        return View(paisExistente);
                    }
                }

                // Validar nombre país
                var nombrePais = pais?.Nombre?.ToUpperInvariant().Trim();

                var nombreError = ValidarNombrePais(nombrePais, pais.ID);

                if (!string.IsNullOrEmpty(nombreError))
                {
                    ModelState.AddModelError(string.Empty, nombreError);
                    return View(paisExistente);
                }

                // Actualizar datos del país
                paisExistente.Nombre = nombrePais;

                await ActualizarDestinos(paisExistente, pais.Destinos ?? new List<Destino>());

                EliminarDestinosNoExistentes(paisExistente, pais.Destinos);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "¡El país se actualizó correctamente!";

                return RedirectToAction(nameof(Index));
            }
            catch (ValidationErrorsException vex)
            {
                ViewData["ValidationErrors"] = vex.Errors;

                foreach (var error in vex.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(pais);
            }
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

        private async Task ActualizarDestinos(Pais paisExistente, ICollection<Destino> destinos)
        {
            var errores = new List<string>();

            if (paisExistente.Destinos == null)
                paisExistente.Destinos = new List<Destino>();

            // Inicializar nombres con los destinos existentes (en mayúsculas y trim)
            var nombresDestinos = paisExistente.Destinos
                .Where(d => !string.IsNullOrWhiteSpace(d?.Nombre))
                .Select(d => d.Nombre.ToUpper().Trim())
                .ToList();

            if (destinos == null)
                return;

            foreach (var destino in destinos)
            {
                if (destino == null || string.IsNullOrWhiteSpace(destino.Nombre))
                    continue;

                var nuevoNombre = destino.Nombre.ToUpper().Trim();
                var nuevaDuracion = destino.DuracionTexto?.ToUpper().Trim() ?? "";

                if (destino.ID > 0)
                {
                    // Actualizar destino existente
                    var destinoExistente = paisExistente.Destinos.FirstOrDefault(d => d.ID == destino.ID);
                    if (destinoExistente == null)
                        continue;

                    if (!string.Equals(destinoExistente.Nombre, nuevoNombre, StringComparison.OrdinalIgnoreCase))
                    {
                        // Validar duplicados en formulario, excluyendo el nombre actual que vamos a cambiar
                        if (nombresDestinos.Contains(nuevoNombre))
                        {
                            errores.Add($"El destino '{destino.Nombre}' está duplicado en el formulario.");
                            continue;
                        }

                        // Validar duplicado en BD (excluyendo este mismo destino por ID)
                        if (_context.Destinos.Any(d => d.Nombre.ToUpper().Trim() == nuevoNombre && d.ID != destino.ID))
                        {
                            errores.Add($"El destino '{destino.Nombre}' ya existe para otro país");
                            continue;
                        }

                        if (!errores.Any())
                        {
                            // Actualiza lista de nombres para futuras validaciones dentro del ciclo
                            nombresDestinos.Remove(destinoExistente.Nombre.ToUpper().Trim());
                            nombresDestinos.Add(nuevoNombre);
                        }
                    }

                    // Actualizar datos
                    destinoExistente.Nombre = nuevoNombre;
                    destinoExistente.DuracionTexto = nuevaDuracion;
                }
                else
                {
                    // Validar duplicados en formulario, excluyendo el nombre actual que vamos a cambiar
                    if (nombresDestinos.Contains(nuevoNombre))
                    {
                        errores.Add($"El destino '{destino.Nombre}' está duplicado en el formulario.");
                        continue;
                    }

                    // Validar duplicado en BD (excluyendo este mismo destino por ID)
                    if (_context.Destinos.Any(d => d.Nombre.ToUpper().Trim() == nuevoNombre && d.ID != destino.ID))
                    {
                        errores.Add($"El destino '{destino.Nombre}' ya existe para otro país");
                        continue;
                    }

                    paisExistente.Destinos.Add(new Destino
                    {
                        Nombre = nuevoNombre,
                        DuracionTexto = nuevaDuracion
                    });

                    nombresDestinos.Add(nuevoNombre);
                }
            }

            if (errores.Count > 0)
                throw new ValidationErrorsException(errores);
        }

        private async Task<string?> ProcesarArchivoBandera(Pais paisExistente, IFormFile banderaFile)
        {
            var extension = Path.GetExtension(banderaFile.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(extension))
            {
                return "Solo se permiten imágenes JPG o PNG.";
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await banderaFile.CopyToAsync(stream);

            paisExistente.Bandera = $"/uploads/{uniqueFileName}";

            return null;
        }

        private string? ValidarNombrePais(string? nombrePais, int paisId)
        {
            if (string.IsNullOrEmpty(nombrePais))
            {
                return "El nombre del país es obligatorio.";
            }

            if (_context.Paises.Any(p => p.Nombre == nombrePais && p.ID != paisId))
            {
                return "No se admiten países duplicados.";
            }

            return null;
        }

        private void EliminarDestinosNoExistentes(Pais paisExistente, ICollection<Destino>? destinosEnviados)
        {
            var idsEnviados = destinosEnviados == null
                ? new List<int>()
                : destinosEnviados.Where(d => d.ID != 0).Select(d => d.ID).ToList();

            var destinosAEliminar = paisExistente.Destinos
                .Where(d => !idsEnviados.Contains(d.ID))
                .ToList();

            foreach (var destino in destinosAEliminar)
            {
                if (destino.ID > 0)
                {
                    _context.Destinos.Remove(destino);
                }
            }
        }
    }
}
