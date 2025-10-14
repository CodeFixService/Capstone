using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;              // Tu contexto EF
using SmartFlow.Web.Models;            // Notificacion, etc.
using System.Linq;
using System.Collections.Generic;

namespace SmartFlow.Web.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        // 🔹 Métricas para el dashboard (solo lectura)
        public int TotalUsuarios { get; private set; }
        public int TotalReservas { get; private set; }
        public int Pendientes { get; private set; }
        public int NotificacionesSinLeer { get; private set; }

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            // ====== Tu lógica actual de notificaciones (se mantiene) ======
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (int.TryParse(usuarioIdStr, out int usuarioId))
            {
                var notificaciones = _context.Notificaciones
                    .Where(n => n.UsuarioId == usuarioId)
                    .OrderByDescending(n => n.FechaCreacion)
                    .ToList();

                ViewData["Notificaciones"] = notificaciones;
                ViewData["UsuarioId"] = usuarioId;
                ViewData["NotificacionesPendientes"] = notificaciones.Count(n => !n.Leida);

                NotificacionesSinLeer = notificaciones.Count(n => !n.Leida);
            }
            else
            {
                ViewData["Notificaciones"] = new List<Notificacion>();
                ViewData["UsuarioId"] = 0;
                ViewData["NotificacionesPendientes"] = 0;

                NotificacionesSinLeer = 0;
            }

            // ====== Métricas simples (NUEVO) ======
            TotalUsuarios = _context.Usuarios.Count();
            TotalReservas = _context.Reservas.Count();
            Pendientes = _context.Reservas.Count(r => r.Estado == "Pendiente");
        }
    }
}
