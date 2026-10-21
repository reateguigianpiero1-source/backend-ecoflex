using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TiendaHerramientas.Models;

namespace TiendaHerramientas.Controllers
{
    public class CarritoController : Controller
    {
        private const string NombreCookie = "carrito_token";
        private TiendaContext db = new TiendaContext();

        // GET: Carrito
        public ActionResult Index()
        {
            var carrito = BuscarCarrito(LeerToken()) ?? new Carrito();
            return View(carrito);
        }

        // POST: Carrito/Agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Agregar(int productoId, int cantidad = 1)
        {
            if (cantidad < 1) cantidad = 1;

            var producto = db.Productos
                .Include(p => p.Inventario)
                .FirstOrDefault(p => p.Id == productoId && p.Activo);

            if (producto == null) return HttpNotFound();

            var token = LeerToken() ?? CrearToken();
            var carrito = BuscarCarrito(token);

            if (carrito == null)
            {
                carrito = new Carrito { TokenSesion = token };
                db.Carritos.Add(carrito);
            }

            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);
            int cantidadFinal = cantidad + (item != null ? item.Cantidad : 0);
            int disponible = producto.Inventario != null ? producto.Inventario.StockDisponible : 0;

            if (cantidadFinal > disponible)
            {
                TempData["Error"] = "Solo hay " + disponible + " unidades disponibles de " + producto.Nombre + ".";
                return RedirectToAction("Details", "Productos", new { id = productoId });
            }

            if (item == null)
            {
                carrito.Items.Add(new ItemCarrito
                {
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioVenta   // se congela el precio de este momento
                });
            }
            else
            {
                item.Cantidad = cantidadFinal;
            }

            carrito.FechaActualizacion = DateTime.Now;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // POST: Carrito/ActualizarCantidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActualizarCantidad(int itemId, int cantidad)
        {
            var item = BuscarItem(itemId);
            if (item == null) return RedirectToAction("Index");

            if (cantidad < 1)
            {
                db.ItemsCarrito.Remove(item);
            }
            else
            {
                int disponible = item.Producto.Inventario != null ? item.Producto.Inventario.StockDisponible : 0;

                if (cantidad > disponible)
                {
                    TempData["Error"] = "Solo hay " + disponible + " unidades disponibles de " + item.Producto.Nombre + ".";
                    return RedirectToAction("Index");
                }

                item.Cantidad = cantidad;
            }

            item.Carrito.FechaActualizacion = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Carrito/Quitar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Quitar(int itemId)
        {
            var item = BuscarItem(itemId);
            if (item != null)
            {
                item.Carrito.FechaActualizacion = DateTime.Now;
                db.ItemsCarrito.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // POST: Carrito/Vaciar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Vaciar()
        {
            var carrito = BuscarCarrito(LeerToken());
            if (carrito != null)
            {
                db.ItemsCarrito.RemoveRange(carrito.Items.ToList());
                carrito.FechaActualizacion = DateTime.Now;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // ---------- Ayudas privadas ----------

        private Carrito BuscarCarrito(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            return db.Carritos
                .Include(c => c.Items.Select(i => i.Producto))
                .FirstOrDefault(c => c.TokenSesion == token && c.Estado == EstadoCarrito.Activo);
        }

        // Solo devuelve el item si pertenece al carrito activo de quien hace la petición
        private ItemCarrito BuscarItem(int itemId)
        {
            var token = LeerToken();
            if (string.IsNullOrEmpty(token)) return null;

            return db.ItemsCarrito
                .Include(i => i.Carrito)
                .Include(i => i.Producto.Inventario)
                .FirstOrDefault(i => i.Id == itemId
                                     && i.Carrito.TokenSesion == token
                                     && i.Carrito.Estado == EstadoCarrito.Activo);
        }

        private string LeerToken()
        {
            var cookie = Request.Cookies[NombreCookie];
            return (cookie != null && !string.IsNullOrEmpty(cookie.Value)) ? cookie.Value : null;
        }

        private string CrearToken()
        {
            var token = Guid.NewGuid().ToString("N");
            Response.Cookies.Add(new HttpCookie(NombreCookie, token)
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(30)
            });
            return token;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
