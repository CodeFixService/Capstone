using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SmartFlow.Web.Data;
using SmartFlow.Web.Helpers;  // 👈 para EmailHelper
using SmartFlow.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFlow.Web.Pages.Admin.Notificaciones
{
    public class NuevaModel : PageModel
    {
        private readonly SmartFlowContext _context;
        private readonly EmailHelper _emailHelper;

        public NuevaModel(SmartFlowContext context, IConfiguration config)
        {
            _context = context;
            _emailHelper = new EmailHelper(config); //  inyectamos el helper de correo
        }

        // 🔹 Campos del formulario
        [BindProperty] public int? UsuarioId { get; set; }
        [BindProperty] public bool EnviarATodos { get; set; }
        [BindProperty] public string Titulo { get; set; } = "";
        [BindProperty] public string Mensaje { get; set; } = "";
        [BindProperty] public bool EnviarCorreo { get; set; }

        // 🔹 Lista para llenar el combo
        public List<SmartFlow.Web.Models.Usuario> Usuarios { get; set; } = new();

        // 🔹 Mensaje de confirmación
        public string MensajeSistema { get; set; } = "";

        public IActionResult OnGet()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Admin" && rol != "Director" && rol != "Coordinador") return RedirectToPage("/Login/Login");

            Usuarios = _context.Usuarios.OrderBy(u => u.Nombre).ToList();
            return Page();
        }

        public IActionResult OnPost()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Admin" && rol != "Director" && rol != "Coordinador") return RedirectToPage("/Login/Login");

            if (string.IsNullOrWhiteSpace(Titulo) || string.IsNullOrWhiteSpace(Mensaje))
            {
                MensajeSistema = " Debe ingresar título y mensaje.";
                OnGet();
                return Page();
            }

            var ahora = DateTime.Now;

            if (EnviarATodos)
            {
                var usuarios = _context.Usuarios.ToList();
                foreach (var u in usuarios)
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

                    // 🔹 Enviar correo si está activado
                    if (EnviarCorreo && !string.IsNullOrEmpty(u.Correo))
                    {
                        try
                        {
                            _emailHelper.EnviarCorreo(u.Correo, Titulo, Mensaje);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al enviar correo a {u.Correo}: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                if (UsuarioId is null || UsuarioId <= 0)
                {
                    MensajeSistema = " Seleccione un usuario o marque 'Enviar a todos'.";
                    OnGet();
                    return Page();
                }

                var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == UsuarioId);
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
                        try
                        {
                            _emailHelper.EnviarCorreo(usuario.Correo, Titulo, Mensaje);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al enviar correo a {usuario.Correo}: {ex.Message}");
                        }
                    }
                }
            }

            _context.SaveChanges();

            // 🔹 Reset del formulario
            Usuarios = _context.Usuarios.OrderBy(u => u.Nombre).ToList();
            UsuarioId = null;
            EnviarATodos = false;
            Titulo = "";
            Mensaje = "";
            MensajeSistema = "✅ Notificación enviada correctamente.";

            return Page();
        }
    }
}
