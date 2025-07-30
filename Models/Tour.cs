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

        public virtual Pais Pais { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una fecha.")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una hora.")]
        public TimeSpan Hora { get; set; }

        [Required(ErrorMessage = "Debe establecer un precio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
        public decimal Precio { get; set; }

      
        [NotMapped]
        public decimal ITBIS => Precio * 0.18M;

      
        [NotMapped]
        public string Duracion => Destino?.DuracionTexto ?? "";

        [NotMapped]
        public DateTime FechaFin => Fecha.Add(Hora).Add(ParseDuracion(Duracion));

       
        [NotMapped]
        public string Estado => FechaFin > DateTime.Now ? "Vigente" : "Vencido";

       
        private TimeSpan ParseDuracion(string texto)
        {
            var duracion = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(texto))
                return duracion;

            var matchDias = Regex.Match(texto, @"(\d+)\s*d[ií]as?");
            if (matchDias.Success)
            {
                int dias = int.Parse(matchDias.Groups[1].Value);
                duracion = duracion.Add(TimeSpan.FromDays(dias));
            }

            var matchHoras = Regex.Match(texto, @"(\d+)\s*horas?");
            if (matchHoras.Success)
            {
                int horas = int.Parse(matchHoras.Groups[1].Value);
                duracion = duracion.Add(TimeSpan.FromHours(horas));
            }

            return duracion;
        }
    }
}
