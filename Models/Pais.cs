using System.ComponentModel.DataAnnotations;

namespace AgenciaDeToursRD.Models
{
    public class Pais
    {

        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre del país es obligatorio")]
        public string Nombre { get; set; }

        public string? Bandera { get; set; }

        public virtual ICollection<Destino>? Destinos { get; set; }
    }
}

