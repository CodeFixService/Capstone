using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System;
using System.Threading.Tasks;

namespace SmartFlow.Web.Helpers
{
    public static class BitacoraHelper
    {
        public static async Task RegistrarAsync(SmartFlowContext context, string usuario, string modulo, string accion, string detalle)
        {
            var log = new Bitacora
            {
                Usuario = usuario,
                Modulo = modulo,
                Accion = accion,
                Detalle = detalle,
                Fecha = DateTime.Now
            };

            context.Bitacoras.Add(log);
            await context.SaveChangesAsync();
        }
    }
}
