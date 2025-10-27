using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFlow.Web.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        // 🔹 Relación con Usuario
        public int UsuarioId { get; set; }

        // 🔹 Relación con Servicio (FK)
        public int ServicioId { get; set; }

        // 🔹 Propiedades de navegación (no obligatorias pero útiles)
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [ForeignKey("ServicioId")]
        public Servicio Servicio { get; set; }

        // 🔹 Fechas de reserva
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // 🔹 Estado actual
        public string Estado { get; set; } = "Pendiente";

        // 🔹 Fecha de creación
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // 🔹 Comentarios
        public string? ComentarioUsuario { get; set; }  // lo escribe el usuario
        public string? ComentarioAdmin { get; set; }    // lo escribe el admin al aprobar o rechazar
    }
}
