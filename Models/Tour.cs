using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace AgenciaDeToursRD.Models
{
    public class Tour
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre del tour es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un destino.")]
        public int DestinoID { get; set; }

        [ForeignKey("DestinoID")]
        public Destino Destino { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un país.")]
        public int PaisID { get; set; }

        public virtual Pais? Pais { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una fecha.")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una hora.")]
        public TimeSpan Hora { get; set; }

        [Required(ErrorMessage = "Debe establecer un precio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
        public decimal Precio { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ITBIS { get; set; } = 0;

        [Column(TypeName = "datetime")]
        public DateTime FechaFin { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string Estado { get; set; } 

        public string Duracion => Destino?.DuracionTexto ?? "";


        public static TimeSpan ParseDuracion(string texto)
        {
            var duracion = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(texto))
                return duracion;

            var matchDias = Regex.Match(texto, @"(\d+)\s*d[ií]as?");
            if (matchDias.Success)
                duracion = TimeSpan.FromDays(int.Parse(matchDias.Groups[1].Value));

            return duracion;
        }

    }
}
