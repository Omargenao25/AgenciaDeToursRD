using AgenciaDeToursRD.Data;
using AgenciaDeToursRD.Excepciones;
using AgenciaDeToursRD.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
                .ThenInclude(d => d.Tours)
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
        public async Task<IActionResult> Create(Pais pais, IFormFile? BanderaFile)
        {
            try
            {
                if (pais.Destinos == null)
                {
                    pais.Destinos = new List<Destino>();
                }

                if (_context.Paises.Any(p => p.Nombre == pais.Nombre.ToUpperInvariant().Trim()))
                {
                    ModelState.AddModelError(string.Empty, "No se admiten países duplicados");
                    return View(pais);
                }

                if (BanderaFile == null || BanderaFile.Length == 0)
                {
                    ModelState.AddModelError("Bandera", "La bandera es obligatoria.");
                    return View(pais);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var extensionBandera = Path.GetExtension(BanderaFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extensionBandera))
                {
                    ModelState.AddModelError("Bandera", "Solo se permiten imágenes JPG o PNG.");
                    return View(pais);
                }

                var banderaFileName = $"{Guid.NewGuid()}{extensionBandera}";
                var banderaPath = Path.Combine(uploadsFolder, banderaFileName);
                using var streamBandera = new FileStream(banderaPath, FileMode.Create);
                await BanderaFile.CopyToAsync(streamBandera);
                var banderaUrl = $"/uploads/{banderaFileName}";

                var destinosValidos = new List<Destino>();
                var nombresDestinos = pais.Destinos
                    .Where(d => !string.IsNullOrWhiteSpace(d.Nombre))
                    .Select(d => d.Nombre.ToUpper().Trim())
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

                    if (destino.DestinoFile != null && destino.DestinoFile.Length > 0)
                    {
                        var ext = Path.GetExtension(destino.DestinoFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(ext))
                        {
                            ModelState.AddModelError(string.Empty, $"La imagen del destino '{destino.Nombre}' tiene una extensión no permitida.");
                            return View(pais);
                        }

                        var destinoFileName = $"{Guid.NewGuid()}{ext}";
                        var destinoFilePath = Path.Combine(uploadsFolder, destinoFileName);
                        using var streamDestino = new FileStream(destinoFilePath, FileMode.Create);
                        await destino.DestinoFile.CopyToAsync(streamDestino);
                        destino.ImagenUrl = $"/uploads/{destinoFileName}";
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
            catch (Exception)
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pais pais, IFormFile? BanderaFile)
        {
            try
            {
                if (pais.Destinos == null)
                    pais.Destinos = new List<Destino>();

                var nombrePais = pais.Nombre?.ToUpperInvariant().Trim();
                if (_context.Paises.Any(p => p.ID != id && p.Nombre == nombrePais))
                {
                    var paisParaFusion = await _context.Paises.Include(p => p.Destinos).FirstOrDefaultAsync(p => p.ID == id);
                    FusionarDatosPersistentes(pais, paisParaFusion);
                    ModelState.AddModelError(string.Empty, "No se admiten países duplicados");
                    return View(pais);
                }

                var paisExistente = await _context.Paises.Include(p => p.Destinos).FirstOrDefaultAsync(p => p.ID == id);
                if (paisExistente == null)
                    return NotFound();

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Bandera
                if (BanderaFile != null && BanderaFile.Length > 0)
                {
                    var extensionBandera = Path.GetExtension(BanderaFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extensionBandera))
                    {
                        FusionarDatosPersistentes(pais, paisExistente);
                        ModelState.AddModelError("Bandera", "Solo se permiten imágenes JPG o PNG.");
                        return View(pais);
                    }

                    var banderaFileName = $"{Guid.NewGuid()}{extensionBandera}";
                    var banderaPath = Path.Combine(uploadsFolder, banderaFileName);
                    using var streamBandera = new FileStream(banderaPath, FileMode.Create);
                    await BanderaFile.CopyToAsync(streamBandera);
                    paisExistente.Bandera = $"/uploads/{banderaFileName}";
                }

                paisExistente.Nombre = nombrePais;

                // Validación de destinos
                var destinosValidos = new List<Destino>();
                var nombresDestinos = pais.Destinos
                    .Where(d => !string.IsNullOrWhiteSpace(d.Nombre))
                    .Select(d => d.Nombre.ToUpper().Trim())
                    .ToList();

                var nombresExistentes = _context.Destinos
                    .Where(d => nombresDestinos.Contains(d.Nombre) && d.PaisId != id)
                    .Select(d => d.Nombre)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var destino in pais.Destinos)
                {
                    destino.Nombre = destino?.Nombre?.ToUpper().Trim();
                    destino.DuracionTexto = destino?.DuracionTexto?.ToUpper().Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(destino?.Nombre)) continue;

                    if (nombresExistentes.Contains(destino.Nombre))
                    {
                        FusionarDatosPersistentes(pais, paisExistente);
                        ModelState.AddModelError(string.Empty, $"El destino '{destino.Nombre}' ya existe y no se admite duplicado.");
                        return View(pais);
                    }

                    if (destinosValidos.Any(d => d.Nombre == destino.Nombre))
                    {
                        FusionarDatosPersistentes(pais, paisExistente);
                        ModelState.AddModelError(string.Empty, $"El destino '{destino.Nombre}' está repetido en el formulario.");
                        return View(pais);
                    }

                    // Imagen del destino
                    if (destino.DestinoFile != null && destino.DestinoFile.Length > 0)
                    {
                        var ext = Path.GetExtension(destino.DestinoFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(ext))
                        {
                            FusionarDatosPersistentes(pais, paisExistente);
                            ModelState.AddModelError(string.Empty, $"La imagen del destino '{destino.Nombre}' tiene una extensión no permitida.");
                            return View(pais);
                        }

                        var destinoFileName = $"{Guid.NewGuid()}{ext}";
                        var destinoFilePath = Path.Combine(uploadsFolder, destinoFileName);
                        using var streamDestino = new FileStream(destinoFilePath, FileMode.Create);
                        await destino.DestinoFile.CopyToAsync(streamDestino);
                        destino.ImagenUrl = $"/uploads/{destinoFileName}";
                    }
                    else
                    {
                        var existente = paisExistente.Destinos.FirstOrDefault(d => d.ID == destino.ID);
                        destino.ImagenUrl = existente?.ImagenUrl;
                    }

                    destino.PaisId = paisExistente.ID;
                    destinosValidos.Add(destino);
                }

                // Eliminar destinos que ya no están en el formulario
                var idsFormulario = destinosValidos.Select(d => d.ID).ToHashSet();
                var destinosAEliminar = paisExistente.Destinos.Where(d => !idsFormulario.Contains(d.ID)).ToList();

                foreach (var destino in destinosAEliminar)
                {
                    _context.Destinos.Remove(destino);
                }

                // Actualizar o agregar destinos
                foreach (var destino in destinosValidos)
                {
                    var existente = paisExistente.Destinos.FirstOrDefault(d => d.ID == destino.ID);
                    if (existente != null)
                    {
                        existente.Nombre = destino.Nombre;
                        existente.DuracionTexto = destino.DuracionTexto;
                        existente.ImagenUrl = destino.ImagenUrl;
                    }
                    else
                    {
                        paisExistente.Destinos.Add(destino);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "¡El país se actualizó correctamente!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var paisParaFusion = await _context.Paises.Include(p => p.Destinos).FirstOrDefaultAsync(p => p.ID == id);
                FusionarDatosPersistentes(pais, paisParaFusion);
                ModelState.AddModelError(string.Empty, $"Error al intentar guardar los datos: {ex.Message}");
                return View(pais);
            }
        }


        private void FusionarDatosPersistentes(Pais paisFormulario, Pais paisExistente)
        {
            paisFormulario.Bandera ??= paisExistente.Bandera;

            if (paisFormulario.Destinos == null)
                paisFormulario.Destinos = new List<Destino>();

            foreach (var destino in paisFormulario.Destinos)
            {
                var existente = paisExistente.Destinos.FirstOrDefault(d => d.ID == destino.ID);
                destino.ImagenUrl ??= existente?.ImagenUrl;
                destino.PaisId = paisExistente.ID;
            }
        }




        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var pais = await _context.Paises
                .Include(p => p.Destinos)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (pais == null)
                return NotFound();

            return View(pais);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pais = await _context.Paises
                .Include(p => p.Destinos)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (pais == null)
                return NotFound();

            if (pais.Destinos != null && pais.Destinos.Any())
            {
                TempData["ErrorMessage"] = "No se puede eliminar un país que tenga destinos asociados.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Paises.Remove(pais);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "El país fue eliminado correctamente.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Error al intentar eliminar el país.";
            }

            return RedirectToAction(nameof(Index));
        }


        private async Task ActualizarDestinos(Pais paisExistente, ICollection<Destino> destinos)
        {
            var errores = new List<string>();

            if (paisExistente.Destinos == null)
                paisExistente.Destinos = new List<Destino>();

            
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
            var destinosOriginales = paisExistente.Destinos.ToList();

            // Si la lista enviada es null, la convertimos en una lista vacía
            var destinosVista = destinosEnviados ?? new List<Destino>();

            foreach (var destino in destinosOriginales)
            {
                bool destinoExisteEnVista = destinosVista.Any(d => d.ID == destino.ID);

                if (!destinoExisteEnVista)
                {
                    // Verificamos si el destino tiene tours asociados
                    bool tieneTours = _context.Tours.Any(t => t.DestinoID == destino.ID);

                    if (tieneTours)
                    {
                        ModelState.AddModelError("", $"El destino '{destino.Nombre}' no puede ser eliminado porque tiene tours asociados.");
                        continue; // No se elimina, se registra el error
                    }

                    _context.Destinos.Remove(destino); // Se elimina si no tiene tours
                }
            }
        }


    }
}
