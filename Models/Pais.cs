namespace AgenciaDeToursRD.Models
{
    public class Pais
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public virtual ICollection<Destino> Destinos { get; set; }
    }
}
