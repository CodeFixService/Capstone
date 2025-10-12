using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SmartFlow.Web.Models
{
    public class Arancel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del arancel es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal Monto { get; set; }

        public string? Descripcion { get; set; }

        // Relación con carrera
        public int? CarreraId { get; set; }
        public Carrera? Carrera { get; set; }
    }
}
