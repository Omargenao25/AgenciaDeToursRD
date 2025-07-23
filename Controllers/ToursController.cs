using AgenciaDeToursRD.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaDeToursRD.Controllers
{
    public class ToursController : Controller
    {
        private readonly AgenciaDeToursDbContext _context;

   
        public ActionResult Index()
        {
            return View();
        }

      
        public ActionResult Details(int id)
        {
            return View();
        }

     
        public ActionResult Create()
        {
            return View();
        }

  
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
    }
}
