using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaHerramientas.Models
{
    public enum TipoMovimiento
    {
        Entrada = 1,      // Compra a proveedor
        Salida = 2,       // Venta
        Ajuste = 3,       // Corrección manual
        Devolucion = 4
    }

    public class Inventario
    {
        // La clave primaria es también la clave foránea al producto (relación 1 a 1 en EF6)
        [Key, ForeignKey("Producto")]
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }
        public virtual Producto Producto { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        [Display(Name = "Stock actual")]
        public int StockActual { get; set; }

        // Unidades apartadas en carritos o pedidos pendientes
        [Range(0, int.MaxValue)]
        [Display(Name = "Stock reservado")]
        public int StockReservado { get; set; }

        // Se alerta cuando el stock disponible baja de este número
        [Range(0, int.MaxValue)]
        [Display(Name = "Stock mínimo")]
        public int StockMinimo { get; set; }

        [StringLength(50)]
        [Display(Name = "Ubicación")]
        public string Ubicacion { get; set; }             // Ej: "Almacén A - Estante 3"

        [DataType(DataType.DateTime)]
        [Display(Name = "Última actualización")]
        public DateTime UltimaActualizacion { get; set; }

        [NotMapped]
        [Display(Name = "Stock disponible")]
        public int StockDisponible
        {
            get { return StockActual - StockReservado; }
        }

        [NotMapped]
        public bool StockBajo
        {
            get { return StockDisponible <= StockMinimo; }
        }

        public virtual ICollection<MovimientoInventario> Movimientos { get; set; }

        public Inventario()
        {
            StockMinimo = 5;
            UltimaActualizacion = DateTime.Now;
            Movimientos = new List<MovimientoInventario>();
        }
    }

    public class MovimientoInventario
    {
        public int Id { get; set; }

        [ForeignKey("Inventario")]
        public int InventarioId { get; set; }
        public virtual Inventario Inventario { get; set; }

        [Display(Name = "Tipo de movimiento")]
        public TipoMovimiento Tipo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }                 // Siempre positiva; el tipo define si suma o resta

        [StringLength(250)]
        [Display(Name = "Motivo")]
        public string Motivo { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; }

        [StringLength(100)]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; }

        public MovimientoInventario()
        {
            Fecha = DateTime.Now;
        }
    }
}
