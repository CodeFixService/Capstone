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
            var rol = HttpContext.Session.GetString("Rol");
            var adminId = HttpContext.Session.GetInt32("UsuarioId");

            if (adminId == null || (rol != "Admin" && rol != "Coordinador"))
                return RedirectToPage("/Login/Login");

            UsuarioId = usuarioId;

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null)
                return RedirectToPage("/Admin/Ayuda/Index");

            NombreUsuario = usuario.Nombre;

            // 🔹 Cargar todos los mensajes del mismo hilo (usuario base)
            Mensajes = _context.ChatMensajes
                .Where(c => c.UsuarioId == usuarioId)
                .OrderBy(c => c.Fecha)
                .ToList();

            // 🔹 Marcar como leídos los mensajes del otro rol
            var rolActual = rol;
            foreach (var c in Mensajes)
            {
                if (rolActual == "Admin" && !c.LeidoPorAdmin && c.EmisorRol != "Admin")
                    c.LeidoPorAdmin = true;
                if (rolActual == "Coordinador" && !c.LeidoPorCoordinador && c.EmisorRol != "Coordinador")
                    c.LeidoPorCoordinador = true;
            }
            _context.SaveChanges();

            return Page();
        }

        public IActionResult OnPost(int usuarioId, string texto)
        {
            var rol = HttpContext.Session.GetString("Rol");
            var userId = HttpContext.Session.GetInt32("UsuarioId");
            if (userId == null) return RedirectToPage("/Login/Login");

            if (string.IsNullOrWhiteSpace(texto))
                return RedirectToPage(new { usuarioId });

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null) return RedirectToPage("/Admin/Ayuda/Index");

            // 🔹 Registrar el mensaje con el rol actual (Admin o Coordinador)
            _context.ChatMensajes.Add(new ChatMensaje
            {
                UsuarioId = usuarioId,
                EmisorRol = rol,
                Texto = texto.Trim(),
                Fecha = DateTime.Now,
                LeidoPorAdmin = rol == "Admin",
                LeidoPorCoordinador = rol == "Coordinador",
                LeidoPorUsuario = false
            });

            // 🔹 Notificación para el otro rol (Admin ↔ Coordinador)
            var destinatarios = _context.Usuarios
                .Where(u => u.Rol == (rol == "Admin" ? "Coordinador" : "Admin"))
                .Select(u => u.Id)
                .ToList();

            foreach (var id in destinatarios)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = id,
                    Titulo = $"Nuevo mensaje de {rol}",
                    Mensaje = $"{rol} envió un mensaje relacionado con {usuario.Nombre}.",
                    Tipo = "Chat",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            _context.SaveChanges();

            MensajeSistema = "✅ Mensaje enviado correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public IActionResult OnGetMensajesParciales(int usuarioId)
        {
            var mensajes = _context.ChatMensajes
                .Where(c => c.UsuarioId == usuarioId)
                .OrderBy(c => c.Fecha)
                .ToList();

            var html = string.Join("", mensajes.Select(m =>
                $"<div class='mb-2'>" +
                $"<small class='text-muted'>{m.Fecha:g}</small><br/>" +
                $"<span class='badge {(m.EmisorRol == "Admin" ? "bg-dark" : m.EmisorRol == "Coordinador" ? "bg-success" : "bg-primary")}'>{m.EmisorRol}</span>" +
                $"<span class='ms-2'>{m.Texto}</span></div>"
            ));

            return Content(html, "text/html");
        }
    }
}
