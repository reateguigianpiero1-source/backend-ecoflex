using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TiendaHerramientas.Models;

namespace TiendaHerramientas.Controllers
{
    public class InventarioController : Controller
    {
        private TiendaContext db = new TiendaContext();

        // GET: Inventario?soloStockBajo=true
        public ActionResult Index(bool soloStockBajo = false)
        {
            var inventarios = db.Inventarios.Include(i => i.Producto);

            if (soloStockBajo)
            {
                inventarios = inventarios.Where(i => (i.StockActual - i.StockReservado) <= i.StockMinimo);
            }

            ViewBag.SoloStockBajo = soloStockBajo;
            return View(inventarios.OrderBy(i => i.Producto.Nombre).ToList());
        }

        // GET: Inventario/Movimientos/5   (5 = id del producto)
        public ActionResult Movimientos(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var inventario = BuscarInventario(id.Value);
            if (inventario == null) return HttpNotFound();

            ViewBag.Inventario = inventario;

            var movimientos = db.Movimientos
                .Where(m => m.InventarioId == id.Value)
                .OrderByDescending(m => m.Fecha)
                .ToList();

            return View(movimientos);
        }

        // GET: Inventario/Registrar/5
        public ActionResult Registrar(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var inventario = BuscarInventario(id.Value);
            if (inventario == null) return HttpNotFound();

            ViewBag.Inventario = inventario;
            return View(new MovimientoInventario { InventarioId = id.Value });
        }

        // POST: Inventario/Registrar
        // Entrada y Devolucion suman. Salida resta. Ajuste fija el stock en la cantidad indicada.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar([Bind(Include = "InventarioId,Tipo,Cantidad,Motivo")] MovimientoInventario movimiento)
        {
            var inventario = BuscarInventario(movimiento.InventarioId);
            if (inventario == null) return HttpNotFound();

            if (ModelState.IsValid)
            {
                switch (movimiento.Tipo)
                {
                    case TipoMovimiento.Entrada:
                    case TipoMovimiento.Devolucion:
                        inventario.StockActual += movimiento.Cantidad;
                        break;

                    case TipoMovimiento.Salida:
                        if (movimiento.Cantidad > inventario.StockDisponible)
                        {
                            ModelState.AddModelError("Cantidad",
                                "No hay suficiente stock disponible. Disponible: " + inventario.StockDisponible);
                        }
                        else
                        {
                            inventario.StockActual -= movimiento.Cantidad;
                        }
                        break;

                    case TipoMovimiento.Ajuste:
                        if (movimiento.Cantidad < inventario.StockReservado)
                        {
                            ModelState.AddModelError("Cantidad",
                                "El nuevo stock no puede ser menor al reservado (" + inventario.StockReservado + ").");
                        }
                        else
                        {
                            inventario.StockActual = movimiento.Cantidad;
                        }
                        break;
                }

                if (ModelState.IsValid)
                {
                    movimiento.Fecha = DateTime.Now;
                    movimiento.Usuario = User.Identity.Name;
                    inventario.UltimaActualizacion = DateTime.Now;

                    db.Movimientos.Add(movimiento);
                    db.SaveChanges();   // guarda movimiento y nuevo stock en una sola transacción

                    return RedirectToAction("Movimientos", new { id = movimiento.InventarioId });
                }
            }

            ViewBag.Inventario = inventario;
            return View(movimiento);
        }

        // GET: Inventario/Configurar/5   (stock mínimo y ubicación)
        public ActionResult Configurar(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var inventario = BuscarInventario(id.Value);
            if (inventario == null) return HttpNotFound();

            return View(inventario);
        }

        // POST: Inventario/Configurar/5
        [HttpPost, ActionName("Configurar")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfigurarPost(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var inventario = BuscarInventario(id.Value);
            if (inventario == null) return HttpNotFound();

            if (TryUpdateModel(inventario, "", new[] { "StockMinimo", "Ubicacion" }))
            {
                inventario.UltimaActualizacion = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(inventario);
        }

        private Inventario BuscarInventario(int productoId)
        {
            return db.Inventarios
                .Include(i => i.Producto)
                .FirstOrDefault(i => i.ProductoId == productoId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
