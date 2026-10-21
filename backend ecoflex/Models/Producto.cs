using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaHerramientas.Models
{
    public enum TipoCategoria
    {
        Herramienta = 1,
        Embalaje = 2
    }

    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(80)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [StringLength(250)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Tipo")]
        public TipoCategoria Tipo { get; set; }

        public virtual ICollection<Producto> Productos { get; set; }

        public Categoria()
        {
            Productos = new List<Producto>();
        }
    }

    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El SKU es obligatorio")]
        [StringLength(40)]
        [Display(Name = "SKU")]
        public string Sku { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [StringLength(1000)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [StringLength(80)]
        [Display(Name = "Marca")]
        public string Marca { get; set; }

        [Range(0, 999999, ErrorMessage = "Precio no válido")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio de venta")]
        public decimal PrecioVenta { get; set; }

        [Range(0, 999999, ErrorMessage = "Precio no válido")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio de costo")]
        public decimal PrecioCosto { get; set; }

        [StringLength(20)]
        [Display(Name = "Unidad de medida")]
        public string UnidadMedida { get; set; }          // Unidad, Caja, Rollo, Paquete...

        [StringLength(100)]
        [Display(Name = "Dimensiones")]
        public string Dimensiones { get; set; }           // Ej: "40x30x20 cm"

        [Display(Name = "Peso (kg)")]
        public decimal? PesoKg { get; set; }

        [StringLength(300)]
        [Display(Name = "Imagen")]
        public string ImagenUrl { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de creación")]
        public DateTime FechaCreacion { get; set; }

        // Relaciones
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }
        public virtual Categoria Categoria { get; set; }

        public virtual Inventario Inventario { get; set; }   // 1 a 1
        public virtual ICollection<ItemCarrito> ItemsCarrito { get; set; }

        public Producto()
        {
            UnidadMedida = "Unidad";
            Activo = true;
            FechaCreacion = DateTime.Now;
            ItemsCarrito = new List<ItemCarrito>();
        }
    }
}