namespace AgenciaDeToursRD.Excepciones
{
    public class ValidationErrorsException : Exception
    {
        public List<string> Errors { get; }

        public ValidationErrorsException(IEnumerable<string> errors)
            : base("Errores de validación encontrados.")
        {
            Errors = errors.ToList();
        }
    }
}
