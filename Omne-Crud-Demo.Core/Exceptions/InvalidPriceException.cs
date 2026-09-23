namespace Omne_Crud_Demo.Core.Exceptions;

public sealed class InvalidPriceException : DomainException
{
    public InvalidPriceException(string message)
        : base(message)
    {
    }
}
