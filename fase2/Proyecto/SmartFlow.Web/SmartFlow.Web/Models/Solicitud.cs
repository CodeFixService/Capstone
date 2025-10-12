using System;
using System.ComponentModel.DataAnnotations;

namespace SmartFlow.Web.Models
{
    public class Solicitud
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El asunto de la solicitud es obligatorio")]
        [StringLength(100)]
        public string Asunto { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // 🔹 Estado: Pendiente, Aprobada, Rechazada
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";

        // 🔹 Relación con Usuario
        [Required]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        // 🔹 Relación con Servicio (opcional)
        public int? ServicioId { get; set; }
        public Servicio? Servicio { get; set; }
        [StringLength(300)]
        public string? Motivo { get; set; } // 🔹 Nuevo campo para comentarios del admin

    }
}
