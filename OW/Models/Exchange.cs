using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OW.Models
{
    /// <summary>
    /// Модель для обмена валюты
    /// </summary>
    public class Exchange
    {
        [Required]
        public string InputCurrency { get; set; }
        [Required]
        public string OutputCurrency { get; set; }
        [Required]
        public decimal Ammount { get; set; }

    }
}
