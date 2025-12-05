using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using System.Linq;
using System;
using System.Collections.Generic;

namespace SmartFlow.Web.Pages.Coordinador.Ayuda
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public IndexModel(SmartFlowContext context) => _context = context;

        public List<Item> Conversaciones { get; set; } = new();

        public class Item
        {
            public int UsuarioId { get; set; }
            public string Nombre { get; set; } = "";
            public string Rol { get; set; } = "";
            public string UltimoTexto { get; set; } = "";
            public DateTime FechaUltimo { get; set; }
            public int NoLeidos { get; set; }
        }

        public void OnGet()
        {
            var rol = HttpContext.Session.GetString("Rol");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null || rol != "Coordinador")
            {
                Response.Redirect("/Login/Login");
                return;
            }

            // 🔹 Obtener la carrera del coordinador
            var carreraId = _context.Usuarios
                .Where(u => u.Id == usuarioId)
                .Select(u => u.CarreraId)
                .FirstOrDefault();

            if (carreraId == null)
            {
                Conversaciones = new();
                return;
            }

            // 🔹 Mostrar estudiantes y admins de su carrera
            Conversaciones = _context.Usuarios
                .Where(u => u.CarreraId == carreraId && (u.Rol == "Usuario"))
                .Select(u => new Item
                {
                    UsuarioId = u.Id,
                    Nombre = u.Nombre,
                    Rol = u.Rol,
                    UltimoTexto = _context.ChatMensajes
                        .Where(c => c.UsuarioId == u.Id)
                        .OrderByDescending(c => c.Fecha)
                        .Select(c => c.Texto)
                        .FirstOrDefault() ?? "-",
                    FechaUltimo = _context.ChatMensajes
                        .Where(c => c.UsuarioId == u.Id)
                        .OrderByDescending(c => c.Fecha)
                        .Select(c => c.Fecha)
                        .FirstOrDefault(),
                    // 🔹 Contador de mensajes no leídos optimizado
                    NoLeidos = _context.ChatMensajes.Count(c =>
                        c.UsuarioId == u.Id &&
                        (
                            // Mensajes nuevos de estudiantes hacia el coordinador
                            (u.Rol == "Usuario" && !c.LeidoPorCoordinador && c.EmisorRol == "Usuario")
                            // Mensajes nuevos de admin hacia el coordinador
                        ))
                })
                .OrderByDescending(x => x.FechaUltimo)
                .ToList();
        }
    }
}
