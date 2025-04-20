
namespace Supabase.Postgrest
{
    [Serializable]
    internal class PostgrestException : Exception
    {
        public PostgrestException()
        {
        }

        public PostgrestException(string? message) : base(message)
        {
        }

        public PostgrestException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}