namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class vw_mtto_movimientos
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int movimiento_id { get; set; }

        [Column("ID Movimiento")]
        [StringLength(10)]
        public string ID_Movimiento { get; set; }

        [Key]
        [Column(Order = 1, TypeName = "datetime2")]
        public DateTime Fecha { get; set; }

        [Key]
        [Column("Tipo de Movimiento", Order = 2)]
        [StringLength(40)]
        public string Tipo_de_Movimiento { get; set; }

        [Key]
        [Column("ID Articulo", Order = 3)]
        [StringLength(20)]
        public string ID_Articulo { get; set; }

        [Key]
        [Column("Nombre del Articulo", Order = 4)]
        [StringLength(250)]
        public string Nombre_del_Articulo { get; set; }

        [Key]
        [Column(Order = 5)]
        public decimal Cantidad { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(80)]
        public string Almacen { get; set; }

        [Column("Almacen Destino")]
        [StringLength(80)]
        public string Almacen_Destino { get; set; }

        [StringLength(120)]
        public string Responsable { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }

        public decimal? stock_previo { get; set; }

        public decimal? stock_resultante { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short signo { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int articulo_id { get; set; }

        [Key]
        [Column(Order = 9)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int tipo_id { get; set; }

        [Key]
        [Column(Order = 10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int almacen_id { get; set; }
    }
}
