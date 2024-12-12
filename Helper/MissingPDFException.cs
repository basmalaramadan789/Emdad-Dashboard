namespace Emdad_Dashboard.Helper
{
    public class MissingPDFException : Exception
    {
        public MissingPDFException(string message) : base(message) { }
    }
}
