namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

/// <summary>
/// Thrown by the domain layer to signal a business-rule violation.
/// Translated to HTTP 400 by the global exception middleware.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

