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
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuarioActual = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (rol != "Admin")
                return RedirectToPage("/Login/Login");

            //  ADMIN GENERAL (sin carrera) → ve todos
            if (rol == "Admin" && usuarioActual.CarreraId == null)
            {
                Usuarios = _context.Usuarios
                    .OrderBy(u => u.Nombre)
                    .ToList();
            }
            else if (rol == "Admin") //  ADMIN DE CARRERA
            {
                Usuarios = _context.Usuarios
        .Where(u => u.CarreraId == usuarioActual.CarreraId || u.CarreraId == null) // 👈 incluye super admin
                    .OrderBy(u => u.Nombre)
                    .ToList();
            }
            else if (rol == "Coordinador") //  COORDINADOR
            {
                Usuarios = _context.Usuarios
                    .Where(u =>
                        u.CarreraId == usuarioActual.CarreraId &&
                        (u.Rol == "Admin" || u.Rol == "Usuario") // estudiante/admin
                    )
                    .OrderBy(u => u.Nombre)
                    .ToList();
            }


            return Page();
        }


        public IActionResult OnPost()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuarioActual = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Admin" && rol != "Director" && rol != "Coordinador")
                return RedirectToPage("/Login/Login");

            if (string.IsNullOrWhiteSpace(Titulo) || string.IsNullOrWhiteSpace(Mensaje))
            {
                MensajeSistema = " Debe ingresar título y mensaje.";
                OnGet();
                return Page();
            }

            var ahora = DateTime.Now;

            
            //  1) DETERMINAR LISTA DE USUARIOS SEGÚN ROL Y CARRERA
            

            List<SmartFlow.Web.Models.Usuario> listaUsuariosNotificar;

            //  ADMIN GENERAL → todos
            if (rol == "Admin" && usuarioActual.CarreraId == null)
            {
                listaUsuariosNotificar = _context.Usuarios.ToList();
            }
            //  ADMIN DE CARRERA → solo su carrera
            else if (rol == "Admin")
            {
                listaUsuariosNotificar = _context.Usuarios
                    .Where(u => u.CarreraId == usuarioActual.CarreraId)
                    .ToList();
            }
            //  COORDINADOR → Admin + Estudiantes de la misma carrera
            else if (rol == "Coordinador")
            {
                listaUsuariosNotificar = _context.Usuarios
                    .Where(u =>
                        u.CarreraId == usuarioActual.CarreraId &&
                        (u.Rol == "Admin" || u.Rol == "Usuario")
                    ).ToList();
            }
            //  DIRECTOR → todos
            else
            {
                listaUsuariosNotificar = _context.Usuarios.ToList();
            }

            
            //  2) ENVÍO A TODOS (pero respetando lo anterior)
            

            if (EnviarATodos)
            {
                foreach (var u in listaUsuariosNotificar)
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
                
                // 🔵 3) ENVÍO INDIVIDUAL (también respetando carrera/rol)
                

                if (UsuarioId is null || UsuarioId <= 0)
                {
                    MensajeSistema = " Seleccione un usuario o marque 'Enviar a todos'.";
                    OnGet();
                    return Page();
                }

                var usuarioDestino = listaUsuariosNotificar.FirstOrDefault(u => u.Id == UsuarioId);

                if (usuarioDestino == null)
                {
                    MensajeSistema = " No tiene permiso para enviar a ese usuario.";
                    OnGet();
                    return Page();
                }

                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = usuarioDestino.Id,
                    Titulo = Titulo,
                    Mensaje = Mensaje,
                    Tipo = "Info",
                    Leida = false,
                    FechaCreacion = ahora
                });

                if (EnviarCorreo && !string.IsNullOrEmpty(usuarioDestino.Correo))
                {
                    try
                    {
                        _emailHelper.EnviarCorreo(usuarioDestino.Correo, Titulo, Mensaje);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al enviar correo a {usuarioDestino.Correo}: {ex.Message}");
                    }
                }
            }

            _context.SaveChanges();

            MensajeSistema = " Notificación enviada correctamente.";
            OnGet();
            return Page();
        }

    }
}
