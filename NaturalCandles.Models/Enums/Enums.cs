using System.ComponentModel.DataAnnotations;

namespace NaturalCandles.Models.Enums
{
    public enum CandleColor
    {
        None,
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
        None,
        Róż,
        Beż
    }

    public enum ShippingMethod
    {
        [Display(Name = "InPost Paczkomat")]
        InPostLocker = 1,

        [Display(Name = "InPost Kurier")]
        InPostCourier = 2,

        [Display(Name = "DPD Kurier")]
        DpdCourier = 3,

        [Display(Name = "ORLEN Paczka")]
        OrlenPaczka = 4,

        [Display(Name = "Odbiór osobisty")]
        LocalPickup = 5
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }

    public enum PaymentMethod
    {
        [Display(Name = "BLIK")]
        Blik = 1,

        [Display(Name = "Przelewy24")]
        Przelewy24 = 2,

        [Display(Name = "Karta płatnicza")]
        Card = 3,

        [Display(Name = "Przelew tradycyjny")]
        BankTransfer = 4,

        [Display(Name = "Płatność przy odbiorze")]
        CashOnDelivery = 5
    }
}