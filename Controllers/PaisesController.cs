using AgenciaDeToursRD.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DestinosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DestinosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: DestinosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DestinosController/Edit/5
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
