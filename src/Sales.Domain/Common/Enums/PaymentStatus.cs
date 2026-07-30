namespace Sales.Domain.Common.Enums;

public enum PaymentStatus
{
    Pending = 1,    // Pendente
    Approved = 2,   // Aprovado
    Declined = 3,   // Recusado
    Refunded = 4,   // Estornado
    Canceled = 5    // Cancelado
}