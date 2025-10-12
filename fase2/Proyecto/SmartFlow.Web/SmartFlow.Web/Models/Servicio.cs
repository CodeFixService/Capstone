using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SmartFlow.Web.Models
{
    public class Servicio
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(250)]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El valor es obligatorio")]
        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal Precio { get; set; }

        public bool Activo { get; set; } = true; // 🔹 permite activar o desactivar sin eliminar
    }
}
