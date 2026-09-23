namespace Omne_Crud_Demo.Core.Exceptions;

public abstract class DomainException : ArgumentException
{
    protected DomainException(string message)
        : base(message)
    {
    }
}
