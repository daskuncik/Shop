using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillingService.Models
{
    public class Billing
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AmmountPaid { get; set; }
    }
}
