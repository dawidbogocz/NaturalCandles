using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalCandles.Models.Enums
{

    public enum CandleColor
    {
        None, // Default if color option is not available or not selected
        Yes,
        No
    }

    public enum CandleType
    {
        Cieniowane,
        Jednolite
    }

    public enum ColorScheme
    {
        None, // Optional if product doesn't use color schemes
        Róż,
        Beż
    }

    public enum ShippingMethod
    {
        PersonalPickup,     // 10 zł
        InPostPaczkomat,    // 20 zł
        DPDKurier,          // 25 zł
        InPostKurier        // TBD
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }

    public enum PaymentMethod
    {
        Card,
        PayPal,
        BankTransfer
    }
}
