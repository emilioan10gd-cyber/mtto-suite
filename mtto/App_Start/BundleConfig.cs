using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Optimization;

namespace mtto
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // En el servidor esta app se publica SOLO con bin/: es un backend de
            // Web API y no sirve vistas MVC, así que Scripts/ y Content/ ni se
            // copian. Los Include de abajo llevan comodín ("{version}", "*"), y
            // para resolverlo System.Web.Optimization tiene que listar la carpeta;
            // si no está, lanza HttpException("El directorio no existe").
            //
            // Eso pasa dentro de Application_Start, así que no rompe una página:
            // impide que la aplicación arranque y TODAS las rutas responden 500,
            // incluida la raíz. Por eso se comprueba antes en vez de confiar en
            // que las carpetas estén.
            if (ExisteCarpeta("~/Scripts"))
            {
                bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                            "~/Scripts/jquery-{version}.js"));

                // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información. De este modo, estará
                // para la producción, use la herramienta de compilación disponible en https://modernizr.com para seleccionar solo las pruebas que necesite.
                bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                            "~/Scripts/modernizr-*"));

                bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                          "~/Scripts/bootstrap.js"));
            }

            if (ExisteCarpeta("~/Content"))
            {
                bundles.Add(new StyleBundle("~/Content/css").Include(
                          "~/Content/bootstrap.css",
                          "~/Content/site.css"));
            }
        }

        private static bool ExisteCarpeta(string rutaVirtual)
        {
            var fisica = HostingEnvironment.MapPath(rutaVirtual);
            return fisica != null && Directory.Exists(fisica);
        }
    }
}
