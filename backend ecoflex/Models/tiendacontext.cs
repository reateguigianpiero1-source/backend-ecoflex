using System.Data.Entity;

namespace TiendaHerramientas.Models
{
    public class TiendaContext : DbContext
    {
        // "TiendaContext" es el nombre de la cadena de conexión en Web.config
        public TiendaContext() : base("name=TiendaContext")
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }

        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<MovimientoInventario> Movimientos { get; set; }

        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<ItemCarrito> ItemsCarrito { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Precisión de los decimales (EF6 no la acepta en [Column])
            modelBuilder.Entity<Producto>().Property(p => p.PrecioVenta).HasPrecision(10, 2);
            modelBuilder.Entity<Producto>().Property(p => p.PrecioCosto).HasPrecision(10, 2);
            modelBuilder.Entity<Producto>().Property(p => p.PesoKg).HasPrecision(8, 3);
            modelBuilder.Entity<ItemCarrito>().Property(i => i.PrecioUnitario).HasPrecision(10, 2);

            // Relación 1 a 1: cada producto tiene un solo registro de inventario
            modelBuilder.Entity<Inventario>()
                .HasRequired(i => i.Producto)
                .WithOptional(p => p.Inventario);

            base.OnModelCreating(modelBuilder);
        }
    }
}
