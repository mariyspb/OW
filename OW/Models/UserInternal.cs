using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OW.Models
{
    public class UserInternal 
    {
        public string Name { get; set; }
        public List<CurrencyInternal> Currencies { get; set; } = new List<CurrencyInternal>();

    }
}
