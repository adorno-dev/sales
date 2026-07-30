using Sales.Domain.Common.Exceptions;

namespace Sales.Domain.Common.Validations;

internal static class Guard
{
    public static void AgainstEmptyGuid(Guid id, string paramName, string? message = null)
    {
        if (id == Guid.Empty)
            throw new DomainException(message ?? $"{paramName} cannot be Guid.Empty.");
    }

    public static void AgainstNull<T>(T? value, string paramName)
    {
        if (value is null)
            throw new DomainException($"{paramName} cannot be null.");
    }

    public static void AgainstNullOrWhiteSpace(string? value, string paramName, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(message ?? $"{paramName} cannot be null or empty.");
    }

    public static void Against<TException>(bool condition, string message)
        where TException : Exception
    {
        if (condition)
            throw (TException)Activator.CreateInstance(typeof(TException), message)!;
    }
}