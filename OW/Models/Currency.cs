using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OW.Models
{
    /// <summary>
    /// Модель для валюты
    /// </summary>
    public class Currency 
    {
        public int Id { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? Ammount { get; set; }

    }
}
