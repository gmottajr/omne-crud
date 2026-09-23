namespace Omne_Crud_Demo.Core.Exceptions;

public sealed class InvalidSkuException : DomainException
{
    public InvalidSkuException(string message)
        : base(message)
    {
    }
}
