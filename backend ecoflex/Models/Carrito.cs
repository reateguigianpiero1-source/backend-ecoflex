using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TiendaHerramientas.Models
{
    public enum EstadoCarrito
    {
        Activo = 1,
        Convertido = 2,   // Se volvió pedido
        Abandonado = 3
    }

    public class Carrito
    {
        public int Id { get; set; }

        // Usuario registrado (puede ser nulo si es visitante)
        [StringLength(128)]
        public string UsuarioId { get; set; }

        // Visitante sin cuenta: se identifica con un token guardado en cookie/sesión
        [StringLength(100)]
        public string TokenSesion { get; set; }

        public EstadoCarrito Estado { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de creación")]
        public DateTime FechaCreacion { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Última actualización")]
        public DateTime FechaActualizacion { get; set; }

        public virtual ICollection<ItemCarrito> Items { get; set; }

        [NotMapped]
        [DataType(DataType.Currency)]
        public decimal Total
        {
            get { return Items.Sum(i => i.Subtotal); }
        }

        [NotMapped]
        [Display(Name = "Cantidad de artículos")]
        public int CantidadTotal
        {
            get { return Items.Sum(i => i.Cantidad); }
        }

        public Carrito()
        {
            Estado = EstadoCarrito.Activo;
            FechaCreacion = DateTime.Now;
            FechaActualizacion = DateTime.Now;
            Items = new List<ItemCarrito>();
        }
    }

    public class ItemCarrito
    {
        public int Id { get; set; }

        public int CarritoId { get; set; }
        public virtual Carrito Carrito { get; set; }

        [Display(Name = "Producto")]
        public int ProductoId { get; set; }
        public virtual Producto Producto { get; set; }

        [Range(1, 10000, ErrorMessage = "La cantidad debe ser al menos 1")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        // Precio al momento de agregar al carrito, por si el producto cambia de precio después
        [DataType(DataType.Currency)]
        [Display(Name = "Precio unitario")]
        public decimal PrecioUnitario { get; set; }

        [NotMapped]
        [DataType(DataType.Currency)]
        public decimal Subtotal
        {
            get { return PrecioUnitario * Cantidad; }
        }

        public ItemCarrito()
        {
            Cantidad = 1;
        }
    }
}
