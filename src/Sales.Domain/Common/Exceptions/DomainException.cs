namespace Sales.Domain.Common.Exceptions;

public class DomainException(string message) : Exception(message)
{
    // Metodo estatico para validacoes de "pre-condicoes"
    public static void When(bool hasError, string errorMessage)
    {
        if (hasError)
            throw new DomainException(errorMessage);
    }
}