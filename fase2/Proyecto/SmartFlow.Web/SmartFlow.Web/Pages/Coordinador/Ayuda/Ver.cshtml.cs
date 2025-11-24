using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFlow.Web.Pages.Coordinador.Ayuda
{
    public class VerModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public VerModel(SmartFlowContext context) => _context = context;

        [BindProperty(SupportsGet = true)]
        public int UsuarioId { get; set; }

        public string NombreUsuario { get; set; } = "";
        public List<ChatMensaje> Mensajes { get; set; } = new();
        public string MensajeSistema { get; set; } = "";

        public IActionResult OnGet()
        {
            var rol = HttpContext.Session.GetString("Rol");
            var coordId = HttpContext.Session.GetInt32("UsuarioId");

            if (coordId == null || rol != "Coordinador")
                return RedirectToPage("/Login/Login");

            // 🔹 Obtener la carrera del coordinador
            var carreraCoord = _context.Usuarios
                .Where(u => u.Id == coordId)
                .Select(u => u.CarreraId)
                .FirstOrDefault();
            // 🔹 Cargar mensajes solo de los usuarios que pertenecen a esa carrera
            // 🔹 Obtener los mensajes relacionados al usuario seleccionado,
            // siempre que pertenezca a la misma carrera
            Mensajes = (from chat in _context.ChatMensajes
                        join user in _context.Usuarios on chat.UsuarioId equals user.Id
                        where user.CarreraId == carreraCoord && chat.UsuarioId == UsuarioId
                        orderby chat.Fecha
                        select chat).ToList();

            // 🔹 Validar que el usuario pertenezca a su carrera
            var usuario = _context.Usuarios
                .Include(u => u.Carrera)
                .FirstOrDefault(u => u.Id == UsuarioId);

            if (usuario == null || usuario.CarreraId != carreraCoord)
                return RedirectToPage("/Coordinador/Ayuda/Index");

            NombreUsuario = usuario.Nombre;

            // 🔹 Cargar mensajes del hilo (usuario + admin) de la misma carrera
           

            // 🔹 Marcar como leídos todos los mensajes del otro rol (Admin o Usuario)
            var noLeidos = Mensajes
                .Where(c => !c.LeidoPorCoordinador && c.EmisorRol != "Coordinador")
                .ToList();

            if (noLeidos.Any())
            {
                foreach (var c in noLeidos)
                    c.LeidoPorCoordinador = true;

                _context.SaveChanges();
            }

            return Page();
        }

        public IActionResult OnPost(string texto)
        {
            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            if (coordId == null) return RedirectToPage("/Login/Login");

            if (string.IsNullOrWhiteSpace(texto))
                return RedirectToPage(new { UsuarioId });

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == UsuarioId);
            if (usuario == null) return RedirectToPage("/Coordinador/Ayuda/Index");

            // 🔹 Crear mensaje del Coordinador (SIEMPRE vinculado al Usuario del hilo)
            var nuevoMensaje = new ChatMensaje
            {
                UsuarioId = UsuarioId, // se mantiene constante
                EmisorRol = "Coordinador",
                Texto = texto.Trim(),
                Fecha = DateTime.Now,
                LeidoPorCoordinador = true,
                LeidoPorUsuario = false,
                LeidoPorAdmin = false
            };

            _context.ChatMensajes.Add(nuevoMensaje);

            // 🔹 Notificar al Admin (no al usuario)
            var admins = _context.Usuarios.Where(u => u.Rol == "Admin").Select(a => a.Id).ToList();
            foreach (var idAdmin in admins)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = idAdmin,
                    Titulo = "Nuevo mensaje del Coordinador",
                    Mensaje = $"El coordinador de {usuario.Carrera?.Nombre ?? "una carrera"} envió un mensaje.",
                    Tipo = "Chat",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            _context.SaveChanges();
            return RedirectToPage(new { UsuarioId });
        }

        public IActionResult OnGetMensajesParciales()
        {
            var mensajes = _context.ChatMensajes
                .Where(c => c.UsuarioId == UsuarioId)
                .OrderBy(c => c.Fecha)
                .ToList();

            // 🔹 Render dinámico
            var html = string.Join("", mensajes.Select(m =>
                $"<div class='mb-2'>" +
                $"<small class='text-muted'>{m.Fecha:g}</small><br/>" +
                $"<span class='badge {(m.EmisorRol == "Coordinador" ? "bg-success" : (m.EmisorRol == "Admin" ? "bg-dark" : "bg-secondary"))}'>{m.EmisorRol}</span>" +
                $"<span class='ms-2'>{m.Texto}</span></div>"
            ));

            return Content(html, "text/html");
        }
    }
}
