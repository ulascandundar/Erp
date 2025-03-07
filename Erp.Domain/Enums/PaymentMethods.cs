using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Domain.Enums;

public static class PaymentMethods
{
    public const string Cash = "Cash";
    public const string CreditCard = "CreditCard";
    public const string Wallet = "Wallet";
    public const string MealCard = "MealCard";
    public const string Discount = "Discount";
    public const string Other = "Other";
}