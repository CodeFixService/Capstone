using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SmartFlow.Web.Pages.Admin.Ayuda
{
    public class VerModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public VerModel(SmartFlowContext context) => _context = context;

        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = "";
        public List<ChatMensaje> Mensajes { get; set; } = new();
        public string MensajeSistema { get; set; } = "";

        public IActionResult OnGet(int usuarioId)
        {
            if (HttpContext.Session.GetString("Rol") != "Admin")
                return RedirectToPage("/Login/Login");

            UsuarioId = usuarioId;
            var u = _context.Usuarios.FirstOrDefault(x => x.Id == usuarioId);
            if (u == null) return RedirectToPage("/Admin/Ayuda/Index");

            NombreUsuario = u.Nombre;

            // Marcar como leídos para admin
            var noLeidosAdmin = _context.ChatMensajes
                .Where(c => c.UsuarioId == usuarioId && !c.LeidoPorAdmin)
                .ToList();
            if (noLeidosAdmin.Any())
            {
                foreach (var c in noLeidosAdmin) c.LeidoPorAdmin = true;
                _context.SaveChanges();
            }

            Mensajes = _context.ChatMensajes
                .Where(c => c.UsuarioId == usuarioId)
                .OrderBy(c => c.Fecha)
                .ToList();

            return Page();
        }

        public IActionResult OnPost(int usuarioId, string texto)
        {
            if (HttpContext.Session.GetString("Rol") != "Admin")
                return RedirectToPage("/Login/Login");

            var u = _context.Usuarios.FirstOrDefault(x => x.Id == usuarioId);
            if (u == null) return RedirectToPage("/Admin/Ayuda/Index");

            _context.ChatMensajes.Add(new ChatMensaje
            {
                UsuarioId = usuarioId,
                EmisorRol = "Admin",
                Texto = texto.Trim(),
                Fecha = DateTime.Now,
                LeidoPorUsuario = false,
                LeidoPorAdmin = true
            });

            // 🔔 Notificar al usuario
            _context.Notificaciones.Add(new Notificacion
            {
                UsuarioId = usuarioId,
                Titulo = "Nueva respuesta de soporte",
                Mensaje = "El Administrador respondió tu mensaje de Ayuda.",
                Tipo = "Info",
                Leida = false,
                FechaCreacion = DateTime.Now
            });

            _context.SaveChanges();

            MensajeSistema = "✅ Respuesta enviada.";
            return RedirectToPage(new { usuarioId });
        }
        public IActionResult OnGetMensajesParciales(int usuarioId)
        {
            if (HttpContext.Session.GetString("Rol") != "Admin")
                return new EmptyResult();

            var mensajes = _context.ChatMensajes
                .Where(c => c.UsuarioId == usuarioId)
                .OrderBy(c => c.Fecha)
                .ToList();

            var html = string.Join("", mensajes.Select(m =>
                $"<div class='mb-2'>" +
                $"<small class='text-muted'>{m.Fecha:g}</small><br/>" +
                $"<span class='badge {(m.EmisorRol == "Usuario" ? "bg-primary" : "bg-dark")}'>{m.EmisorRol}</span>" +
                $"<span class='ms-2'>{m.Texto}</span></div>"
            ));

            return Content(html, "text/html");
        }

    }
}
