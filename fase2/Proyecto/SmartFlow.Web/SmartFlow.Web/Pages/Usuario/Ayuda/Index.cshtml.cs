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

            //  Obtener datos del usuario
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null) return RedirectToPage();

            //  Buscar coordinador de la misma carrera
            var coordinador = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Coordinador" && u.CarreraId == usuario.CarreraId);

            //  Buscar admin asociado a esa carrera (opcional)
            var admin = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Admin" && u.CarreraId == usuario.CarreraId);

            //  Crear mensaje
            _context.ChatMensajes.Add(new ChatMensaje
            {
                UsuarioId = usuarioId.Value,
                CarreraId = usuario.CarreraId,
                CoordinadorId = coordinador?.Id,
                AdminId = admin?.Id,
                EmisorRol = "Usuario",
                DestinatarioRol = "Coordinador",
                Texto = texto.Trim(),
                Fecha = DateTime.Now,
                LeidoPorUsuario = true,
                LeidoPorCoordinador = false,
                LeidoPorAdmin = false
            });

            //  Crear notificación para el coordinador
            if (coordinador != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = coordinador.Id,
                    Titulo = "Nuevo mensaje de un estudiante",
                    Mensaje = $"{usuario.Nombre} te ha enviado un mensaje de ayuda.",
                    Tipo = "Alerta",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            _context.SaveChanges();

            MensajeSistema = " Mensaje enviado.";
            return RedirectToPage(); // recarga el hilo actualizado
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

            var html = string.Join("", mensajes.Select(m =>
                $"<div class='mb-2'>" +
                $"<small class='text-muted'>{m.Fecha:g}</small><br/>" +
                $"<span class='badge {(m.EmisorRol == "Usuario" ? "bg-primary" : m.EmisorRol == "Coordinador" ? "bg-success" : "bg-dark")}'>{m.EmisorRol}</span>" +
                $"<span class='ms-2'>{m.Texto}</span></div>"
            ));

            return Content(html, "text/html");
        }


    }
}
