namespace AgenciaDeToursRD.Models
{
    public class Destino
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public int PaisID { get; set; }
        public string DuracionTexto { get; set; }

        public virtual Pais Pais { get; set; }
    }
}
