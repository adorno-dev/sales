namespace Sales.Domain.Common.Base;

public abstract class ValueObject
{
    // Método que deve ser implementado pelas classes derivadas para retornar
    // todos os atributos que definem a igualdade estrutural.
    protected abstract IEnumerable<object?> GetEqualityComponents();

    // Implementação de igualdade profunda (estrutural).
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    // Combina os HashCodes de todos os componentes para gerar
    // uma representação única deste Value Object.
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
            hash.Add(component);

        return hash.ToHashCode();
    }

    // Compara Value Objects pelos seus valores, e não pela referência de memória.
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    // Operador de negação da igualdade.
    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);
}