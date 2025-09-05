namespace ChatGPTExport.Validators
{
    public class ValidationException : Exception
    {
        public ValidationException() : base("Validation errors found") { }
    }
}
