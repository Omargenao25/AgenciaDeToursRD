namespace AgenciaDeToursRD.Models
{
    public class Destino
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public int PaisId { get; internal set; }
        public string DuracionTexto { get; set; }

        public virtual Pais Pais { get; set; }
        public ICollection<Tour> Tours { get; set; }
    }
}
