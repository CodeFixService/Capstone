using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SmartFlow.Web.Pages.Usuario.Ayuda
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public IndexModel(SmartFlowContext context) => _context = context;

        public List<ChatMensaje> Mensajes { get; set; } = new();
        public string MensajeSistema { get; set; } = "";

        public IActionResult OnGet()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Usuario") return RedirectToPage("/Login/Login");

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToPage("/Login/Login");

            // Marcar como leídos (lado usuario)
            var noLeidos = _context.ChatMensajes
                .Where(c => c.UsuarioId == usuarioId && !c.LeidoPorUsuario)
                .ToList();
            if (noLeidos.Any())
            {
                foreach (var c in noLeidos) c.LeidoPorUsuario = true;
                _context.SaveChanges();
            }

            Mensajes = _context.ChatMensajes
                .Where(c => c.UsuarioId == usuarioId)
                .OrderBy(c => c.Fecha)
                .ToList();

            return Page();
        }

        public IActionResult OnPost(string texto)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToPage("/Login/Login");
            if (string.IsNullOrWhiteSpace(texto)) return RedirectToPage();

            _context.ChatMensajes.Add(new ChatMensaje
            {
                UsuarioId = usuarioId.Value,
                EmisorRol = "Usuario",
                Texto = texto.Trim(),
                Fecha = DateTime.Now,
                LeidoPorUsuario = true,
                LeidoPorAdmin = false
            });

            // 🔔 Notificar a todos los admins
            var admins = _context.Usuarios.Where(u => u.Rol == "Admin").Select(a => a.Id).ToList();
            foreach (var idAdmin in admins)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = idAdmin,
                    Titulo = "Nuevo mensaje de soporte",
                    Mensaje = "Un usuario envió un mensaje en Ayuda.",
                    Tipo = "Alerta",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            _context.SaveChanges();

            MensajeSistema = "✅ Mensaje enviado.";
            return RedirectToPage(); // recarga para ver el hilo actualizado
        }
        public IActionResult OnGetMensajesParciales()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new EmptyResult();

            var mensajes = _context.ChatMensajes
                .Where(c => c.UsuarioId == usuarioId)
                .OrderBy(c => c.Fecha)
                .ToList();

            // devolvemos solo el HTML de los mensajes
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
