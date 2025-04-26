using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalCandles.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; } // e.g. Card, PayPal

        public string Status { get; set; } // Pending, Completed, Failed
    }
}
