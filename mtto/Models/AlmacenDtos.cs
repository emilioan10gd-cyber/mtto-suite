using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mtto.Models
{
    // ── Vista: inventario total por artículo ──────────────────────────────
    [Table("vw_alm_inventario")]
    public class vw_alm_inventario
    {
        [Key]
        public int articulo_id { get; set; }
        public string clave_ssa { get; set; }
        public string nombre { get; set; }
        public string unidad_medida { get; set; }
        public bool es_cpm { get; set; }
        public decimal? precio_unitario { get; set; }
        public decimal stock_minimo { get; set; }
        public decimal? stock_maximo { get; set; }
        public string programa { get; set; }
        public bool activo { get; set; }
        public DateTime actualizado_en { get; set; }
        public decimal stock_actual { get; set; }
        public int lotes_con_existencia { get; set; }
        public DateTime? caducidad_proxima { get; set; }
        public decimal valor_total { get; set; }
        public string nivel { get; set; }
    }

    // ── Vista: lotes con estado de caducidad ──────────────────────────────
    [Table("vw_alm_lotes")]
    public class vw_alm_lotes
    {
        [Key]
        public int lote_id { get; set; }
        public int articulo_id { get; set; }
        public string lote { get; set; }
        public DateTime? caducidad { get; set; }
        public decimal cantidad { get; set; }
        public DateTime creado_en { get; set; }
        public string clave_ssa { get; set; }
        public string nombre { get; set; }
        public string unidad_medida { get; set; }
        public int? dias_para_caducar { get; set; }
        public string estado_caducidad { get; set; }
    }

    // ── Vista: movimientos con detalle ────────────────────────────────────
    [Table("vw_alm_movimientos")]
    public class vw_alm_movimientos
    {
        [Key]
        public int movimiento_id { get; set; }
        public string tipo { get; set; }
        public string ajuste_subtipo { get; set; }
        public DateTime fecha { get; set; }
        public decimal cantidad { get; set; }
        public string vale { get; set; }
        public string programa { get; set; }
        public string orden_suministro { get; set; }
        public string area_destino { get; set; }
        public string entregado_a { get; set; }
        public string notas { get; set; }
        public string imagen_base64 { get; set; }
        public DateTime creado_en { get; set; }
        public string lote { get; set; }
        public DateTime? caducidad { get; set; }
        public int articulo_id { get; set; }
        public string clave_ssa { get; set; }
        public string nombre { get; set; }
        public string unidad_medida { get; set; }
        public string proveedor { get; set; }
        public string usuario { get; set; }
    }

    // ── DTOs de respuesta ─────────────────────────────────────────────────
    public class AlmArticuloDto
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; }
        public bool EsCpm { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal? StockMaximo { get; set; }
        public string Programa { get; set; }
        public bool Activo { get; set; }
        public decimal StockActual { get; set; }
        public int LotesConExistencia { get; set; }
        public DateTime? CaducidadProxima { get; set; }
        public decimal ValorTotal { get; set; }
        public string Nivel { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public string ImagenBase64 { get; set; }  // solo poblado en Obtener (no en listado)
    }

    public class AlmLoteDto
    {
        public int Id { get; set; }
        public int ArticuloId { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; }
        public string Lote { get; set; }
        public DateTime? Caducidad { get; set; }
        public decimal Cantidad { get; set; }
        public int? DiasParaCaducar { get; set; }
        public string EstadoCaducidad { get; set; }
    }

    public class AlmMovimientoDto
    {
        public int Id { get; set; }
        public int ArticuloId { get; set; }
        public string Tipo { get; set; }
        public string AjusteSubtipo { get; set; }
        public DateTime Fecha { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; }
        public string Lote { get; set; }
        public DateTime? Caducidad { get; set; }
        public decimal Cantidad { get; set; }
        public string Vale { get; set; }
        public string Programa { get; set; }
        public string OrdenSuministro { get; set; }
        public string AreaDestino { get; set; }
        public string EntregadoA { get; set; }
        public string Notas { get; set; }
        public string Proveedor { get; set; }
        public string Usuario { get; set; }
        public string ImagenBase64 { get; set; }
    }

    public class AlmProveedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Rfc { get; set; }
    }

    public class AlmConsumoAreaDto
    {
        public string Area { get; set; }
        public decimal TotalSalidas { get; set; }
        public int NumMovimientos { get; set; }
    }

    public class AlmDashboardDto
    {
        public int TotalArticulos { get; set; }
        public int ArticulosSinStock { get; set; }
        public int ArticulosBajoMinimo { get; set; }
        public int ArticulosNormal { get; set; }
        public int ArticulosSobreMaximo { get; set; }
        public int LotesCriticos { get; set; }      // ≤30 días — retiro urgente (NOM-241)
        public int LotesPorVencer90 { get; set; }   // 31-90 días — FEFO estricto
        public int LotesPorVencer180 { get; set; }  // 91-180 días — vigilancia activa (NOM-059)
        public int LotesVencidos { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public int MovimientosMes { get; set; }
        public List<AlmConsumoAreaDto> ConsumoMesPorArea { get; set; }
        public List<AlmMovimientoDto> UltimosMovimientos { get; set; }
    }

    // ── DTOs de entrada ───────────────────────────────────────────────────
    public class AlmGuardarArticuloDto
    {
        [Required]
        [StringLength(20)]
        public string Clave { get; set; }

        [Required]
        [StringLength(2000)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(80)]
        public string Unidad { get; set; }

        public bool EsCpm { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal? StockMaximo { get; set; }

        [StringLength(40)]
        public string Programa { get; set; }

        public string ImagenBase64 { get; set; }   // null = no tocar; "" = borrar
    }

    /// <summary>Area de destino con las personas que han recibido ahi.</summary>
    public class AlmAreaDto
    {
        public string Area { get; set; }
        public List<string> Receptores { get; set; }
    }

    // ── Caja (punto de venta): varios articulos, un solo destino ──────────
    public class AlmCajaLineaDto
    {
        [Required]
        public string ClaveArticulo { get; set; }

        /// <summary>Lote exacto. Si viene vacio se surte por FEFO.</summary>
        public string Lote { get; set; }

        [Range(0.0001, double.MaxValue)]
        public decimal Cantidad { get; set; }
    }

    public class AlmRegistrarCajaDto
    {
        [Required]
        public string Tipo { get; set; }          // Salida | Merma | Transferencia

        [StringLength(80)]
        public string AreaDestino { get; set; }

        [StringLength(160)]
        public string EntregadoA { get; set; }

        [StringLength(40)]
        public string Programa { get; set; }

        [StringLength(60)]
        public string Vale { get; set; }

        [StringLength(500)]
        public string Notas { get; set; }

        public DateTime? Fecha { get; set; }
        public string ImagenBase64 { get; set; }

        [Required]
        public List<AlmCajaLineaDto> Items { get; set; }
    }

    public class AlmCajaResultadoDto
    {
        public bool Ok { get; set; }
        public int Registrados { get; set; }
        public List<string> Errores { get; set; }
        public List<AlmMovimientoDto> Movimientos { get; set; }
    }

    public class AlmClaveDto
    {
        public int ArticuloId { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; }
        // Solo viene poblado cuando el término de búsqueda coincidió con un
        // lote (y no con la clave/nombre) — así el cliente sabe con cuál
        // lote precargar la salida, en vez de caer al FEFO por defecto.
        public string LoteCoincidente { get; set; }
        // Va junto con LoteCoincidente: dos artículos distintos pueden traer
        // el mismo texto de lote con caducidades distintas, así que hace
        // falta la fecha para que quien busca sepa cuál es cuál.
        public DateTime? LoteCoincidenteCaducidad { get; set; }
    }

    public class AlmRegistrarMovimientoDto
    {
        [Required]
        public string Tipo { get; set; }          // Entrada | Salida | Merma | Ajuste

        public string AjusteSubtipo { get; set; } // "Agregar" | "Reducir" | "Exacto" — solo Ajuste

        [Required]
        public string ClaveArticulo { get; set; } // buscar por clave_ssa

        [Required]
        [StringLength(60)]
        public string Lote { get; set; }

        public DateTime? Caducidad { get; set; }

        [Required]
        [Range(0, double.MaxValue)]               // 0 permitido para Ajuste Exacto
        public decimal Cantidad { get; set; }

        public DateTime? Fecha { get; set; }

        public string Proveedor { get; set; }     // nombre (se inserta si no existe)
        public string Rfc { get; set; }
        public string Vale { get; set; }
        public string Programa { get; set; }
        public string OrdenSuministro { get; set; }
        public string AreaDestino { get; set; }
        public string EntregadoA { get; set; }
        public string Notas { get; set; }
        public string ImagenBase64 { get; set; }  // data URL opcional (foto de merma, etc.)

        // Solo aplican a Entrada — formato SSA de inventario semanal.
        public string CluesOrigen { get; set; }
        public string FuenteFinanciamiento { get; set; }
        public string PartidaPresupuestal { get; set; }

        // Precio unitario del artículo, capturado al recibirlo (así llega
        // directo desde la factura/remisión en vez de tener que ir aparte a
        // Catálogos a editarlo). Solo se aplica si viene con valor y es Entrada.
        public decimal? PrecioUnitario { get; set; }
    }

    public class AlmProveedorRfcDto
    {
        public int Id { get; set; }
        public int ProveedorId { get; set; }
        public string Rfc { get; set; }
        public bool EsActivo { get; set; }
    }

    public class AlmProveedorConRfcDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string RfcActivo { get; set; }  // el RFC que está marcado como activo
        public List<AlmProveedorRfcDto> Rfc { get; set; }  // todos los RFC históricos
    }
}
