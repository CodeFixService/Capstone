using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Models;

namespace SmartFlow.Web.Data
{
    public class SmartFlowContext : DbContext
    {
        public SmartFlowContext(DbContextOptions<SmartFlowContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Carrera> Carreras { get; set; }
        public DbSet<Arancel> Aranceles { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<ChatMensaje> ChatMensajes { get; set; }






    }
}
