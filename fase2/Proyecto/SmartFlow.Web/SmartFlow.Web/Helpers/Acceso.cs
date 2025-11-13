using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SmartFlow.Web.Helpers
{
    public class Acceso
    {
        private readonly RequestDelegate _next;

        public Acceso(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();
            var session = context.Session;
            var rol = session.GetString("UsuarioRol");

            // 🟢 Permitir libre acceso al Login y contenido estático
            if (path.StartsWith("/login") || path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/lib"))
            {
                await _next(context);
                return;
            }

            //  Bloquear si no hay sesión
            if (string.IsNullOrEmpty(rol))
            {
                context.Response.Redirect("/Login/Login");
                return;
            }

            // Evitar bucle si ya está en /Admin
            if ((rol == "Admin" || rol == "Director" || rol == "Coordinador") && path.StartsWith("/admin"))
            {
                await _next(context);
                return;
            }



            //  Si el rol es "Usuario" pero entra a "/Admin", redirigir
            if (rol == "Usuario" && path.StartsWith("/admin"))
            {
                context.Response.Redirect("/Usuario/Index");
                return;
            }


            //  Si pasa todos los filtros, continuar
            await _next(context);
        }
    }
}
