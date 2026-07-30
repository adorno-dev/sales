namespace Sales.Domain.Common.Enums;

public enum OrderStatus
{
    Pending = 1,            // Pendente
    PaymentConfirmed = 2,   // Pagamento Confirmado
    Picking = 3,            // Em Separacao
    Shipped = 4,            // Enviado
    Delivered = 5,          // Entregue
    Canceled = 6            // Cancelado
}