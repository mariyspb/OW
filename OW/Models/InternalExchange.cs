using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OW.Models
{
    public class InternalExchange
    {
        /// <summary>
        /// Снятие
        /// </summary>
        [Required]
        public bool Decrease { get; set; }
        [Required]
        public string CurrencyCode { get; set; }
        [Required]
        public decimal Ammount { get; set; }
    }
}
