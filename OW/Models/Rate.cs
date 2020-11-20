using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OW.Models
{
    /// <summary>
    /// Модель коэф валюты
    /// </summary>
    public class Rate
    {
        public Dictionary<string, string>  rates { get; set; }
        public string @base { get; set; }
        public string date { get; set; }
    }


}
