namespace Sales.Domain.Common.Enums;

public enum PaymentMethod
{
    CreditCard = 1,     // Cartao Credito
    DebitCard = 2,      // Cartao Debito
    Pix = 3,            // Pix
    BankSlip = 4,       // Boleto
    BankTransfer = 5    // Transferencia Bancaria
}