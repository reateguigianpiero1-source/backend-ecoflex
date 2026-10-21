using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TiendaHerramientas.Models;

namespace TiendaHerramientas.Controllers
{
    public class CategoriasController : Controller
    {
        private TiendaContext db = new TiendaContext();

        // GET: Categorias
        public ActionResult Index()
        {
            var categorias = db.Categorias
                .Include(c => c.Productos)
                .OrderBy(c => c.Nombre)
                .ToList();

            return View(categorias);
        }

        // GET: Categorias/Create
        public ActionResult Create()
        {
            return View(new Categoria());
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nombre,Descripcion,Tipo")] Categoria categoria)
        {
            if (db.Categorias.Any(c => c.Nombre == categoria.Nombre))
            {
                ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre.");
            }

            if (ModelState.IsValid)
            {
                db.Categorias.Add(categoria);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        // GET: Categorias/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var categoria = db.Categorias.Find(id.Value);
            if (categoria == null) return HttpNotFound();

            return View(categoria);
        }

        // POST: Categorias/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var categoria = db.Categorias.Find(id.Value);
            if (categoria == null) return HttpNotFound();

            if (TryUpdateModel(categoria, "", new[] { "Nombre", "Descripcion", "Tipo" }))
            {
                if (db.Categorias.Any(c => c.Nombre == categoria.Nombre && c.Id != categoria.Id))
                {
                    ModelState.AddModelError("Nombre", "Ya existe otra categoría con ese nombre.");
                }
                else
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            return View(categoria);
        }

        // GET: Categorias/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var categoria = db.Categorias.Include(c => c.Productos).FirstOrDefault(c => c.Id == id.Value);
            if (categoria == null) return HttpNotFound();

            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var categoria = db.Categorias.Include(c => c.Productos).FirstOrDefault(c => c.Id == id);
            if (categoria == null) return HttpNotFound();

            // Si se borra una categoría con productos, EF borraría también los productos
            if (categoria.Productos.Any())
            {
                TempData["Error"] = "No se puede eliminar \"" + categoria.Nombre + "\" porque tiene productos. Cambia la categoría de esos productos primero.";
                return RedirectToAction("Index");
            }

            db.Categorias.Remove(categoria);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}