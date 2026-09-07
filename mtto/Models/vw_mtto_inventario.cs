namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class vw_mtto_inventario
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int articulo_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string ID { get; set; }

        [Key]
        [Column("Nombre del Articulo", Order = 2)]
        [StringLength(250)]
        public string Nombre_del_Articulo { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(80)]
        public string Categoria { get; set; }

        [Key]
        [Column("Unidad de Medida", Order = 4)]
        [StringLength(40)]
        public string Unidad_de_Medida { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(80)]
        public string Almacen { get; set; }

        [StringLength(150)]
        public string Proveedor { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(40)]
        public string Estado { get; set; }

        [Key]
        [Column("Stock Actual", Order = 7)]
        public decimal Stock_Actual { get; set; }

        [Key]
        [Column("Stock Minimo", Order = 8)]
        public decimal Stock_Minimo { get; set; }

        [Column("Stock Maximo")]
        public decimal? Stock_Maximo { get; set; }

        [Column("Costo Unitario")]
        public decimal? Costo_Unitario { get; set; }

        [Column("Valor Total")]
        public decimal? Valor_Total { get; set; }

        [Key]
        [Column("Ultima Actualizacion", Order = 9, TypeName = "datetime2")]
        public DateTime Ultima_Actualizacion { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(12)]
        public string Nivel { get; set; }

        [Key]
        [Column(Order = 11)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int categoria_id { get; set; }

        [Key]
        [Column(Order = 12)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int unidad_id { get; set; }

        [Key]
        [Column(Order = 13)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int almacen_id { get; set; }

        public int? proveedor_id { get; set; }

        [Key]
        [Column(Order = 14)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int estado_id { get; set; }
    }
}
