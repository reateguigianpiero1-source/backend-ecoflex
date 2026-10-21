using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TiendaHerramientas.Models;

namespace TiendaHerramientas.Controllers
{
    public class ProductosController : Controller
    {
        private TiendaContext db = new TiendaContext();

        // Campos que se permiten recibir del formulario (evita "overposting")
        private static readonly string[] CamposEditables =
        {
            "Sku", "Nombre", "Descripcion", "Marca", "PrecioVenta", "PrecioCosto",
            "UnidadMedida", "Dimensiones", "PesoKg", "ImagenUrl", "Activo", "CategoriaId"
        };

        // GET: Productos?buscar=martillo&categoriaId=2
        public ActionResult Index(string buscar, int? categoriaId)
        {
            var productos = db.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Inventario)
                .Where(p => p.Activo);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                productos = productos.Where(p => p.Nombre.Contains(buscar) || p.Sku.Contains(buscar));
            }

            if (categoriaId.HasValue)
            {
                productos = productos.Where(p => p.CategoriaId == categoriaId.Value);
            }

            ViewBag.Buscar = buscar;
            ViewBag.CategoriaId = new SelectList(db.Categorias.OrderBy(c => c.Nombre), "Id", "Nombre", categoriaId);

            return View(productos.OrderBy(p => p.Nombre).ToList());
        }

        // GET: Productos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var producto = db.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Inventario)
                .FirstOrDefault(p => p.Id == id.Value);

            if (producto == null) return HttpNotFound();
            return View(producto);
        }

        // GET: Productos/Create
        public ActionResult Create()
        {
            CargarCategorias(null);
            return View(new Producto());
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Sku,Nombre,Descripcion,Marca,PrecioVenta,PrecioCosto,UnidadMedida,Dimensiones,PesoKg,ImagenUrl,Activo,CategoriaId")] Producto producto)
        {
            if (db.Productos.Any(p => p.Sku == producto.Sku))
            {
                ModelState.AddModelError("Sku", "Ya existe un producto con ese SKU.");
            }

            if (ModelState.IsValid)
            {
                producto.FechaCreacion = DateTime.Now;
                db.Productos.Add(producto);

                // Cada producto nuevo nace con su registro de inventario en cero
                db.Inventarios.Add(new Inventario { Producto = producto });

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var producto = db.Productos.Find(id.Value);
            if (producto == null) return HttpNotFound();

            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var producto = db.Productos.Find(id.Value);
            if (producto == null) return HttpNotFound();

            if (TryUpdateModel(producto, "", CamposEditables))
            {
                if (db.Productos.Any(p => p.Sku == producto.Sku && p.Id != producto.Id))
                {
                    ModelState.AddModelError("Sku", "Ya existe otro producto con ese SKU.");
                }
                else
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        // GET: Productos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var producto = db.Productos.Include(p => p.Categoria).FirstOrDefault(p => p.Id == id.Value);
            if (producto == null) return HttpNotFound();

            return View(producto);
        }

        // POST: Productos/Delete/5
        // Baja lógica: el producto se desactiva, no se borra, para no romper carritos ni historial
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var producto = db.Productos.Find(id);
            if (producto == null) return HttpNotFound();

            producto.Activo = false;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void CargarCategorias(int? seleccionada)
        {
            ViewBag.CategoriaId = new SelectList(db.Categorias.OrderBy(c => c.Nombre), "Id", "Nombre", seleccionada);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}