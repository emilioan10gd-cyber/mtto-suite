namespace mtto.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class mtto_movimiento
    {
        [Key]
        public int movimiento_id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [StringLength(10)]
        public string folio { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime fecha { get; set; }

        public int tipo_id { get; set; }

        public int articulo_id { get; set; }

        public decimal cantidad { get; set; }

        public int almacen_id { get; set; }

        public int? almacen_destino_id { get; set; }

        [StringLength(120)]
        public string responsable { get; set; }

        [StringLength(500)]
        public string observaciones { get; set; }

        public decimal? stock_previo { get; set; }

        public decimal? stock_resultante { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime creado_en { get; set; }

        [StringLength(120)]
        public string creado_por { get; set; }

        public virtual mtto_almacen mtto_almacen { get; set; }

        public virtual mtto_almacen mtto_almacen1 { get; set; }

        public virtual mtto_articulo mtto_articulo { get; set; }

        public virtual mtto_tipo_movimiento mtto_tipo_movimiento { get; set; }
    }
}
