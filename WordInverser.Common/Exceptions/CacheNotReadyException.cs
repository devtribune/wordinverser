namespace WordInverser.Common.Exceptions;

public class CacheNotReadyException : Exception
{
    public CacheNotReadyException() 
        : base("The application is still initializing. Memory cache is not ready yet. Please try again in a few moments.")
    {
    }

    public CacheNotReadyException(string message) : base(message)
    {
    }

    public CacheNotReadyException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
