using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SmartFlow.Web.Data;
using SmartFlow.Web.Helpers;
using SmartFlow.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFlow.Web.Pages.Coordinador.Notificaciones
{
    public class NuevaModel : PageModel
    {
        private readonly SmartFlowContext _context;
        private readonly EmailHelper _emailHelper;

        public NuevaModel(SmartFlowContext context, IConfiguration config)
        {
            _context = context;
            _emailHelper = new EmailHelper(config);
        }

        [BindProperty] public int? UsuarioId { get; set; }
        [BindProperty] public bool EnviarATodos { get; set; }
        [BindProperty] public string Titulo { get; set; } = "";
        [BindProperty] public string Mensaje { get; set; } = "";
        [BindProperty] public bool EnviarCorreo { get; set; }

        public List<SmartFlow.Web.Models.Usuario> Usuarios { get; set; } = new();
        public string MensajeSistema { get; set; } = "";

        public IActionResult OnGet()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Coordinador") return RedirectToPage("/Login/Login");

            // Solo usuarios de la carrera del coordinador
            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            var carreraCoord = _context.Usuarios
                .Where(u => u.Id == coordId)
                .Select(u => u.CarreraId)
                .FirstOrDefault();

                Usuarios = _context.Usuarios
                .Where(u =>
                    // Estudiantes de la carrera
                    (u.CarreraId == carreraCoord && u.Rol == "Usuario") ||

                    // Director de Carrera (Admin con misma carrera)
                    (u.Rol == "Admin" && u.CarreraId == carreraCoord) ||

                    // SuperAdmin (Admin sin carrera asignada)
                    (u.Rol == "Admin" && u.CarreraId == null)
                )
                .OrderBy(u => u.Nombre)
                .ToList();


            return Page();
        }

        public IActionResult OnPost()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Coordinador") return RedirectToPage("/Login/Login");

            if (string.IsNullOrWhiteSpace(Titulo) || string.IsNullOrWhiteSpace(Mensaje))
            {
                MensajeSistema = "Debe ingresar título y mensaje.";
                OnGet();
                return Page();
            }

            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            var carreraCoord = _context.Usuarios
                .Where(u => u.Id == coordId)
                .Select(u => u.CarreraId)
                .FirstOrDefault();

            var ahora = DateTime.Now;

            if (EnviarATodos)
            {
                Usuarios = _context.Usuarios.Where(u =>
        // Estudiantes de la carrera
        (u.CarreraId == carreraCoord && u.Rol == "Usuario") ||

        // Director de Carrera (Admin con misma carrera)
        (u.Rol == "Admin" && u.CarreraId == carreraCoord) ||

        // SuperAdmin (Admin sin carrera asignada)
        (u.Rol == "Admin" && u.CarreraId == null)
    )
    .OrderBy(u => u.Nombre)
    .ToList();


                foreach (var u in Usuarios)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        UsuarioId = u.Id,
                        Titulo = Titulo,
                        Mensaje = Mensaje,
                        Tipo = "Info",
                        Leida = false,
                        FechaCreacion = ahora
                    });

                    if (EnviarCorreo && !string.IsNullOrEmpty(u.Correo))
                    {
                        try { _emailHelper.EnviarCorreo(u.Correo, Titulo, Mensaje); }
                        catch (Exception ex) { Console.WriteLine($"Error al enviar correo a {u.Correo}: {ex.Message}"); }
                    }
                }
            }
            else
            {
                if (UsuarioId is null || UsuarioId <= 0)
                {
                    MensajeSistema = "Seleccione un usuario o marque 'Enviar a todos'.";
                    OnGet();
                    return Page();
                }

                var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == UsuarioId && u.CarreraId == carreraCoord);
                if (usuario != null)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        UsuarioId = usuario.Id,
                        Titulo = Titulo,
                        Mensaje = Mensaje,
                        Tipo = "Info",
                        Leida = false,
                        FechaCreacion = ahora
                    });

                    if (EnviarCorreo && !string.IsNullOrEmpty(usuario.Correo))
                    {
                        try { _emailHelper.EnviarCorreo(usuario.Correo, Titulo, Mensaje); }
                        catch (Exception ex) { Console.WriteLine($"Error al enviar correo a {usuario.Correo}: {ex.Message}"); }
                    }
                }
            }

            _context.SaveChanges();

            // Registrar en bitácora
            _context.Bitacoras.Add(new Bitacora
            {
                Usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Coordinador",
                Accion = "Envió notificación",
                Modulo = "Notificaciones - Coordinador",
                Detalle = $"El coordinador envió una notificación con título '{Titulo}' a su carrera.",
                Fecha = DateTime.Now
            });
            _context.SaveChanges();

            // Reset del formulario
            OnGet();
            UsuarioId = null;
            EnviarATodos = false;
            Titulo = "";
            Mensaje = "";
            MensajeSistema = "✅ Notificación enviada correctamente.";

            return Page();
        }
    }
}
