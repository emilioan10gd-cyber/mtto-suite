namespace mtto.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
        {
        }

        public virtual DbSet<app_usuario> app_usuario { get; set; }
        public virtual DbSet<app_sesion> app_sesion { get; set; }
        public virtual DbSet<bio_area> bio_area { get; set; }
        public virtual DbSet<bio_categoria_equipo> bio_categoria_equipo { get; set; }
        public virtual DbSet<bio_categoria_insumo> bio_categoria_insumo { get; set; }
        public virtual DbSet<bio_estado_equipo> bio_estado_equipo { get; set; }
        public virtual DbSet<bio_equipo> bio_equipo { get; set; }
        public virtual DbSet<bio_insumo> bio_insumo { get; set; }
        public virtual DbSet<bio_lote> bio_lote { get; set; }
        public virtual DbSet<bio_tipo_movimiento> bio_tipo_movimiento { get; set; }
        public virtual DbSet<bio_movimiento> bio_movimiento { get; set; }
        public virtual DbSet<bio_mantenimiento> bio_mantenimiento { get; set; }
        public virtual DbSet<bio_manual> bio_manual { get; set; }
        public virtual DbSet<bio_falla> bio_falla { get; set; }
        public virtual DbSet<vw_bio_inventario> vw_bio_inventario { get; set; }
        public virtual DbSet<vw_bio_lotes> vw_bio_lotes { get; set; }
        public virtual DbSet<vw_bio_insumos> vw_bio_insumos { get; set; }
        public virtual DbSet<vw_bio_mantenimiento> vw_bio_mantenimiento { get; set; }
        public virtual DbSet<cateter_articulo> cateter_articulo { get; set; }
        public virtual DbSet<cateter_categoria> cateter_categoria { get; set; }
        public virtual DbSet<cateter_clave_descripcion> cateter_clave_descripcion { get; set; }
        public virtual DbSet<cateter_lote> cateter_lote { get; set; }
        public virtual DbSet<cateter_tipo_movimiento> cateter_tipo_movimiento { get; set; }
        public virtual DbSet<cateter_ubicacion> cateter_ubicacion { get; set; }
        public virtual DbSet<cateter_movimiento_estado> cateter_movimiento_estado { get; set; }
        public virtual DbSet<cateter_terapia_registro> cateter_terapia_registro { get; set; }
        public virtual DbSet<cateter_terapia_evento> cateter_terapia_evento { get; set; }
        public virtual DbSet<cpm_clave> cpm_clave { get; set; }
        public virtual DbSet<vw_cateter_dashboard_kpi> vw_cateter_dashboard_kpi { get; set; }
        public virtual DbSet<vw_cateter_inventario> vw_cateter_inventario { get; set; }
        public virtual DbSet<vw_cateter_lotes> vw_cateter_lotes { get; set; }
        public virtual DbSet<vw_cateter_movimientos> vw_cateter_movimientos { get; set; }
        public virtual DbSet<vw_cateter_movimientos_mes> vw_cateter_movimientos_mes { get; set; }
        public virtual DbSet<vw_cateter_resumen_categoria> vw_cateter_resumen_categoria { get; set; }
        public virtual DbSet<mtto_almacen> mtto_almacen { get; set; }
        public virtual DbSet<mtto_articulo> mtto_articulo { get; set; }
        public virtual DbSet<mtto_categoria> mtto_categoria { get; set; }
        public virtual DbSet<mtto_estado_articulo> mtto_estado_articulo { get; set; }
        public virtual DbSet<mtto_movimiento> mtto_movimiento { get; set; }
        public virtual DbSet<mtto_precio_proveedor> mtto_precio_proveedor { get; set; }
        public virtual DbSet<mtto_proveedor> mtto_proveedor { get; set; }
        public virtual DbSet<mtto_tipo_movimiento> mtto_tipo_movimiento { get; set; }
        public virtual DbSet<mtto_unidad_medida> mtto_unidad_medida { get; set; }
        public virtual DbSet<vw_mtto_bajo_stock> vw_mtto_bajo_stock { get; set; }
        public virtual DbSet<vw_mtto_distribucion_estado> vw_mtto_distribucion_estado { get; set; }
        public virtual DbSet<vw_mtto_inventario> vw_mtto_inventario { get; set; }
        public virtual DbSet<vw_mtto_movimientos> vw_mtto_movimientos { get; set; }
        public virtual DbSet<vw_mtto_movimientos_mes> vw_mtto_movimientos_mes { get; set; }
        public virtual DbSet<vw_mtto_resumen_categoria> vw_mtto_resumen_categoria { get; set; }
        public virtual DbSet<alm_articulo> alm_articulo { get; set; }
        public virtual DbSet<alm_proveedor> alm_proveedor { get; set; }
        public virtual DbSet<alm_proveedor_rfc> alm_proveedor_rfc { get; set; }
        public virtual DbSet<alm_lote> alm_lote { get; set; }
        public virtual DbSet<alm_movimiento> alm_movimiento { get; set; }
        public virtual DbSet<alm_config> alm_config { get; set; }
        public virtual DbSet<vw_alm_inventario> vw_alm_inventario { get; set; }
        public virtual DbSet<vw_alm_lotes> vw_alm_lotes { get; set; }
        public virtual DbSet<vw_alm_movimientos> vw_alm_movimientos { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<app_usuario>()
                .Property(e => e.creado_en)
                .HasPrecision(0);

            modelBuilder.Entity<app_usuario>()
                .Property(e => e.ultimo_acceso)
                .HasPrecision(0);

            modelBuilder.Entity<app_sesion>()
                .Property(e => e.creado_en)
                .HasPrecision(0);

            modelBuilder.Entity<app_sesion>()
                .Property(e => e.expira_en)
                .HasPrecision(0);

            modelBuilder.Entity<app_sesion>()
                .HasRequired(s => s.app_usuario)
                .WithMany()
                .HasForeignKey(s => s.usuario_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<mtto_almacen>()
                .Property(e => e.creado_en)
                .HasPrecision(0);

            modelBuilder.Entity<mtto_almacen>()
                .HasMany(e => e.mtto_articulo)
                .WithRequired(e => e.mtto_almacen)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<mtto_almacen>()
                .HasMany(e => e.mtto_movimiento)
                .WithRequired(e => e.mtto_almacen)
                .HasForeignKey(e => e.almacen_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<mtto_almacen>()
                .HasMany(e => e.mtto_movimiento1)
                .WithOptional(e => e.mtto_almacen1)
                .HasForeignKey(e => e.almacen_destino_id);

            modelBuilder.Entity<mtto_articulo>()
                .Property(e => e.stock_actual)
                .HasPrecision(18, 3);

            modelBuilder.Entity<mtto_articulo>()
                .Property(e => e.stock_minimo)
                .HasPrecision(18, 3);

            modelBuilder.Entity<mtto_articulo>()
                .Property(e => e.stock_maximo)
                .HasPrecision(18, 3);

            modelBuilder.Entity<mtto_articulo>()
                .Property(e => e.costo_unitario)
                .HasPrecision(18, 4);

            modelBuilder.Entity<mtto_articulo>()
                .Property(e => e.actualizado_en)
                .HasPrecision(0);

            modelBuilder.Entity<mtto_articulo>()
                .Property(e => e.creado_en)
                .HasPrecision(0);

            modelBuilder.Entity<mtto_articulo>()
                .HasMany(e => e.mtto_movimiento)
                .WithRequired(e => e.mtto_articulo)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<mtto_categoria>()
                .Property(e => e.creado_en)
                .HasPrecision(0);

            modelBuilder.Entity<mtto_categoria>()
                .HasMany(e => e.mtto_articulo)
                .WithRequired(e => e.mtto_categoria)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<mtto_estado_articulo>()
                .HasMany(e => e.mtto_articulo)
                .WithRequired(e => e.mtto_estado_articulo)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<mtto_movimiento>()
                .Property(e => e.folio)
                .IsUnicode(false);

            modelBuilder.Entity<mtto_movimiento>()
                .Property(e => e.fecha)
                .HasPrecision(0);

            modelBuilder.Entity<mtto_movimiento>()
                .Property(e => e.cantidad)
                .HasPrecision(18, 3);

            modelBuilder.Entity<mtto_movimiento>()
                .Property(e => e.stock_previo)
                .HasPrecision(18, 3);

            modelBuilder.Entity<mtto_movimiento>()
                .Property(e => e.stock_resultante)
                .HasPrecision(18, 3);

            modelBuilder.Entity<mtto_movimiento>()
                .Property(e => e.creado_en)
                .HasPrecision(0);

            modelBuilder.Entity<mtto_proveedor>()
                .Property(e => e.creado_en)
                .HasPrecision(0);

            modelBuilder.Entity<mtto_precio_proveedor>()
                .Property(e => e.cantidad_minima)
                .HasPrecision(18, 3);

            modelBuilder.Entity<mtto_precio_proveedor>()
                .Property(e => e.precio_unitario)
                .HasPrecision(18, 4);

            modelBuilder.Entity<mtto_precio_proveedor>()
                .Property(e => e.actualizado_en)
                .HasPrecision(0);

            modelBuilder.Entity<mtto_precio_proveedor>()
                .Property(e => e.creado_en)
                .HasPrecision(0);

            modelBuilder.Entity<mtto_precio_proveedor>()
                .HasRequired(p => p.mtto_articulo)
                .WithMany()
                .HasForeignKey(p => p.articulo_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<mtto_precio_proveedor>()
                .HasRequired(p => p.mtto_proveedor)
                .WithMany()
                .HasForeignKey(p => p.proveedor_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<mtto_tipo_movimiento>()
                .HasMany(e => e.mtto_movimiento)
                .WithRequired(e => e.mtto_tipo_movimiento)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<mtto_unidad_medida>()
                .HasMany(e => e.mtto_articulo)
                .WithRequired(e => e.mtto_unidad_medida)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<vw_mtto_bajo_stock>()
                .Property(e => e.stock_actual)
                .HasPrecision(18, 3);

            modelBuilder.Entity<vw_mtto_bajo_stock>()
                .Property(e => e.stock_minimo)
                .HasPrecision(18, 3);

            modelBuilder.Entity<vw_mtto_bajo_stock>()
                .Property(e => e.faltante)
                .HasPrecision(19, 3);

            modelBuilder.Entity<vw_mtto_distribucion_estado>()
                .Property(e => e.valor_total)
                .HasPrecision(38, 2);

            modelBuilder.Entity<vw_mtto_inventario>()
                .Property(e => e.Stock_Actual)
                .HasPrecision(18, 3);

            modelBuilder.Entity<vw_mtto_inventario>()
                .Property(e => e.Stock_Minimo)
                .HasPrecision(18, 3);

            modelBuilder.Entity<vw_mtto_inventario>()
                .Property(e => e.Stock_Maximo)
                .HasPrecision(18, 3);

            modelBuilder.Entity<vw_mtto_inventario>()
                .Property(e => e.Costo_Unitario)
                .HasPrecision(18, 4);

            modelBuilder.Entity<vw_mtto_inventario>()
                .Property(e => e.Ultima_Actualizacion)
                .HasPrecision(0);

            modelBuilder.Entity<vw_mtto_movimientos>()
                .Property(e => e.ID_Movimiento)
                .IsUnicode(false);

            modelBuilder.Entity<vw_mtto_movimientos>()
                .Property(e => e.Fecha)
                .HasPrecision(0);

            modelBuilder.Entity<vw_mtto_movimientos>()
                .Property(e => e.Cantidad)
                .HasPrecision(18, 3);

            modelBuilder.Entity<vw_mtto_movimientos>()
                .Property(e => e.stock_previo)
                .HasPrecision(18, 3);

            modelBuilder.Entity<vw_mtto_movimientos>()
                .Property(e => e.stock_resultante)
                .HasPrecision(18, 3);

            modelBuilder.Entity<vw_mtto_movimientos_mes>()
                .Property(e => e.unidades_entrada)
                .HasPrecision(38, 3);

            modelBuilder.Entity<vw_mtto_movimientos_mes>()
                .Property(e => e.unidades_salida)
                .HasPrecision(38, 3);

            modelBuilder.Entity<vw_mtto_resumen_categoria>()
                .Property(e => e.total_existencias)
                .HasPrecision(38, 3);

            modelBuilder.Entity<vw_mtto_resumen_categoria>()
                .Property(e => e.valor_total)
                .HasPrecision(38, 2);

            ConfigurarCateter(modelBuilder);
            ConfigurarBiomedico(modelBuilder);
        }

        /// <summary>
        /// Biomédico. Área nueva e independiente: comparte la base pero sus
        /// tablas y vistas son propias (prefijo bio_), igual que Catéter.
        /// </summary>
        private static void ConfigurarBiomedico(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<bio_area>().Property(e => e.creado_en).HasPrecision(0);
            modelBuilder.Entity<bio_categoria_equipo>().Property(e => e.creado_en).HasPrecision(0);
            modelBuilder.Entity<bio_categoria_insumo>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<bio_equipo>()
                .HasRequired(e => e.bio_area)
                .WithMany()
                .HasForeignKey(e => e.area_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bio_equipo>()
                .HasOptional(e => e.bio_categoria_equipo)
                .WithMany()
                .HasForeignKey(e => e.categoria_id);

            modelBuilder.Entity<bio_equipo>()
                .HasRequired(e => e.bio_estado_equipo)
                .WithMany()
                .HasForeignKey(e => e.estado_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bio_equipo>().Property(e => e.actualizado_en).HasPrecision(0);
            modelBuilder.Entity<bio_equipo>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<bio_equipo>()
                .HasMany(e => e.bio_mantenimiento)
                .WithRequired(m => m.bio_equipo)
                .HasForeignKey(m => m.equipo_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bio_equipo>()
                .HasMany(e => e.bio_manual)
                .WithRequired(m => m.bio_equipo)
                .HasForeignKey(m => m.equipo_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bio_equipo>()
                .HasMany(e => e.bio_falla)
                .WithRequired(f => f.bio_equipo)
                .HasForeignKey(f => f.equipo_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bio_insumo>()
                .HasOptional(i => i.bio_categoria_insumo)
                .WithMany()
                .HasForeignKey(i => i.categoria_id);

            modelBuilder.Entity<bio_insumo>().Property(e => e.stock_minimo).HasPrecision(18, 3);
            modelBuilder.Entity<bio_insumo>().Property(e => e.stock_maximo).HasPrecision(18, 3);
            modelBuilder.Entity<bio_insumo>().Property(e => e.actualizado_en).HasPrecision(0);
            modelBuilder.Entity<bio_insumo>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<bio_insumo>()
                .HasMany(i => i.bio_lote)
                .WithRequired(l => l.bio_insumo)
                .HasForeignKey(l => l.insumo_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bio_lote>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<bio_movimiento>()
                .HasRequired(m => m.bio_tipo_movimiento)
                .WithMany()
                .HasForeignKey(m => m.tipo_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bio_movimiento>()
                .HasRequired(m => m.bio_insumo)
                .WithMany()
                .HasForeignKey(m => m.insumo_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bio_movimiento>()
                .HasRequired(m => m.bio_lote)
                .WithMany()
                .HasForeignKey(m => m.lote_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bio_movimiento>().Property(e => e.fecha).HasPrecision(0);
            modelBuilder.Entity<bio_movimiento>().Property(e => e.cantidad).HasPrecision(18, 3);
            modelBuilder.Entity<bio_movimiento>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<bio_mantenimiento>().Property(e => e.actualizado_en).HasPrecision(0);
            modelBuilder.Entity<bio_mantenimiento>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<bio_manual>().Property(e => e.subido_en).HasPrecision(0);

            modelBuilder.Entity<bio_falla>().Property(e => e.fecha).HasPrecision(0);
            modelBuilder.Entity<bio_falla>().Property(e => e.actualizado_en).HasPrecision(0);
            modelBuilder.Entity<bio_falla>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<vw_bio_inventario>().Property(e => e.actualizado_en).HasPrecision(0);
            modelBuilder.Entity<vw_bio_inventario>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<vw_bio_lotes>().Property(e => e.existencia).HasPrecision(38, 3);
            modelBuilder.Entity<vw_bio_lotes>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<vw_bio_insumos>().Property(e => e.stock_minimo).HasPrecision(18, 3);
            modelBuilder.Entity<vw_bio_insumos>().Property(e => e.stock_maximo).HasPrecision(18, 3);
            modelBuilder.Entity<vw_bio_insumos>().Property(e => e.existencia_total).HasPrecision(38, 3);
            modelBuilder.Entity<vw_bio_insumos>().Property(e => e.actualizado_en).HasPrecision(0);
            modelBuilder.Entity<vw_bio_insumos>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<vw_bio_mantenimiento>().Property(e => e.actualizado_en).HasPrecision(0);
            modelBuilder.Entity<vw_bio_mantenimiento>().Property(e => e.creado_en).HasPrecision(0);
        }

        /// <summary>
        /// Clínica de Catéter. Va aparte porque es otro módulo: comparte la base
        /// (y mtto_unidad_medida) pero sus tablas y vistas son propias.
        /// </summary>
        private static void ConfigurarCateter(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<cpm_clave>()
                .Property(e => e.cantidad_mensual)
                .HasPrecision(18, 3);

            modelBuilder.Entity<cateter_articulo>()
                .Property(e => e.stock_minimo)
                .HasPrecision(18, 3);

            modelBuilder.Entity<cateter_articulo>()
                .Property(e => e.stock_maximo)
                .HasPrecision(18, 3);

            modelBuilder.Entity<cateter_articulo>()
                .HasRequired(a => a.cateter_categoria)
                .WithMany(c => c.cateter_articulo)
                .HasForeignKey(a => a.categoria_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<cateter_articulo>()
                .HasRequired(a => a.mtto_unidad_medida)
                .WithMany()
                .HasForeignKey(a => a.unidad_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<cateter_lote>()
                .HasRequired(l => l.cateter_articulo)
                .WithMany(a => a.cateter_lote)
                .HasForeignKey(l => l.articulo_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<cateter_clave_descripcion>()
                .ToTable("cateter_clave_descripcion")
                .HasKey(c => c.Clave);

            modelBuilder.Entity<vw_cateter_inventario>().Property(e => e.en_almacen).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_inventario>().Property(e => e.en_stock).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_inventario>().Property(e => e.total).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_inventario>().Property(e => e.total_vigente).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_inventario>().Property(e => e.total_vencido).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_inventario>().Property(e => e.stock_minimo).HasPrecision(18, 3);
            modelBuilder.Entity<vw_cateter_inventario>().Property(e => e.stock_maximo).HasPrecision(18, 3);
            modelBuilder.Entity<vw_cateter_inventario>().Property(e => e.cpm_mensual).HasPrecision(18, 3);
            modelBuilder.Entity<vw_cateter_inventario>().Property(e => e.actualizado_en).HasPrecision(0);

            modelBuilder.Entity<vw_cateter_lotes>().Property(e => e.en_almacen).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_lotes>().Property(e => e.en_stock).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_lotes>().Property(e => e.total).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_lotes>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<vw_cateter_movimientos>().Property(e => e.folio).IsUnicode(false);
            modelBuilder.Entity<vw_cateter_movimientos>().Property(e => e.fecha).HasPrecision(0);
            modelBuilder.Entity<vw_cateter_movimientos>().Property(e => e.creado_en).HasPrecision(0);
            modelBuilder.Entity<vw_cateter_movimientos>().Property(e => e.cantidad).HasPrecision(18, 3);
            modelBuilder.Entity<vw_cateter_movimientos>().Property(e => e.saldo_origen_previo).HasPrecision(18, 3);
            modelBuilder.Entity<vw_cateter_movimientos>().Property(e => e.saldo_origen_resultante).HasPrecision(18, 3);
            modelBuilder.Entity<vw_cateter_movimientos>().Property(e => e.saldo_destino_previo).HasPrecision(18, 3);
            modelBuilder.Entity<vw_cateter_movimientos>().Property(e => e.saldo_destino_resultante).HasPrecision(18, 3);

            modelBuilder.Entity<vw_cateter_dashboard_kpi>().Property(e => e.total_piezas).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_dashboard_kpi>().Property(e => e.piezas_vigentes).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_dashboard_kpi>().Property(e => e.piezas_vencidas).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_dashboard_kpi>().Property(e => e.piezas_en_almacen).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_dashboard_kpi>().Property(e => e.piezas_en_stock).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_dashboard_kpi>().Property(e => e.piezas_por_vencer_30).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_dashboard_kpi>().Property(e => e.piezas_por_vencer_90).HasPrecision(38, 3);

            modelBuilder.Entity<vw_cateter_resumen_categoria>().Property(e => e.total_existencias).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_resumen_categoria>().Property(e => e.total_vigente).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_resumen_categoria>().Property(e => e.total_vencido).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_resumen_categoria>().Property(e => e.en_almacen).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_resumen_categoria>().Property(e => e.en_stock).HasPrecision(38, 3);

            modelBuilder.Entity<vw_cateter_movimientos_mes>().Property(e => e.unidades_entrada).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_movimientos_mes>().Property(e => e.unidades_salida).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_movimientos_mes>().Property(e => e.unidades_merma).HasPrecision(38, 3);
            modelBuilder.Entity<vw_cateter_movimientos_mes>().Property(e => e.unidades_traslado).HasPrecision(38, 3);

            modelBuilder.Entity<cateter_terapia_registro>().Property(e => e.fecha).HasPrecision(0);
            modelBuilder.Entity<cateter_terapia_registro>().Property(e => e.creado_en).HasPrecision(0);

            modelBuilder.Entity<cateter_terapia_evento>().Property(e => e.creado_en).HasPrecision(0);
            modelBuilder.Entity<cateter_terapia_evento>().Property(e => e.actualizado_en).HasPrecision(0);
        }
    }
}
