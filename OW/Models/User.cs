using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OW.Models
{
    /// <summary>
    /// Модель пользователя
    /// </summary>
    public class User 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Currency> Currencies { get; set; } = new List<Currency>();
    }
}
