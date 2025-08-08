using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciaDeToursRD.Models
{
    public class Destino
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre del destino es obligatorio.")]
        public string Nombre { get; set; }

        [Required]
        public int PaisId { get; set; }

        [ForeignKey("PaisId")]
        public virtual Pais? Pais { get; set; }

        public string? DuracionTexto { get; set; }

        public string? ImagenUrl { get; set; }

        [NotMapped]
        public IFormFile? DestinoFile { get; set; }



        public ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}
