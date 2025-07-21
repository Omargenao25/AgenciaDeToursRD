namespace AgenciaDeToursRD.Models
{
    public class Tour
    {
        public int ID { get; set; }
        public string Nombre { get; set; }

        public int PaisID { get; set; }
        public int DestinoID { get; set; }

        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }

        public decimal Precio { get; set; }

        // Propiedades calculadas
        public decimal ITBIS => Precio * 0.18M;

        public string Duracion => Destino?.DuracionTexto ?? "Sin destino";

        public DateTime FechaFin => Fecha + Hora + ParseDuracion(Duracion);

        public string Estado => FechaFin > DateTime.Now ? "Vigente" : "Vencido";

        public Pais Pais { get; set; }
        public Destino Destino { get; set; }

        private TimeSpan ParseDuracion(string texto)
        {

            return TimeSpan.FromDays(2).Add(TimeSpan.FromHours(4));
        }
    }
}
