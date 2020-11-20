using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OW.Models;
using RestSharp;

namespace OW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly WalletContext _context;

        public WalletController(WalletContext context)
        {
            _context = context;

            //для тестировки
            //_context.Database.EnsureDeleted();
            //_context.Database.EnsureCreated();
            if (!_context.Users.Any())
            {
                var user1 = new User()
                {
                    Name = "Mark",
                    Currencies = new List<Currency>() { new Currency() { CurrencyCode = "RUB", Ammount = Convert.ToDecimal(10000) } }
                };

                var user2 = new User()
                {
                    Name = "Polo",
                    Currencies = new List<Currency>() { new Currency() { CurrencyCode = "IDR", Ammount = Convert.ToDecimal(300) } }


                };

                _context.Users.AddRange(user1, user2);

                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Получения курса валют
        /// </summary>
        /// <param name="currency">Валюта, по которой считается курс </param>
        /// <returns></returns>
        [HttpGet("Rate")]
        public async Task<IActionResult> GetRateAync(string currency)
        {
            if (currency == null || !Enum.IsDefined(typeof(CurrencyEnum), currency))
                currency = CurrencyEnum.RUB.ToString();
            var client = new RestClient($"https://api.exchangeratesapi.io/latest?base={currency}");
            var request = new RestRequest(Method.GET);
            IRestResponse response = await client.ExecuteAsync(request);


            return Ok(response.Content);
        }

        /// <summary>
        /// Обмен валюты
        /// </summary>
        /// <param name="userid">пользователь</param>
        /// <param name="exchange">Модель обмена курса валют</param>
        /// <returns></returns>
        [HttpPut("Exchange/{userid}")]
        public async Task<IActionResult> PutExchange(int userid, Exchange exchange)
        {
            if (!Enum.IsDefined(typeof(CurrencyEnum), exchange?.InputCurrency)
                || !Enum.IsDefined(typeof(CurrencyEnum), exchange?.OutputCurrency))
            {
                return BadRequest();
            }

            var user = await _context.Users.Include(c => c.Currencies).Where(i => i.Id == userid).FirstOrDefaultAsync();
            if (user is null
                || !user.Currencies.Any(x => x.CurrencyCode.Equals(exchange.InputCurrency))
                || user.Currencies.Where(x => x.CurrencyCode.Equals(exchange.InputCurrency)).First().Ammount < exchange.Ammount)
            {
                return NotFound();
            }

            var client = new RestClient($"https://api.exchangeratesapi.io/latest?symbols={exchange.OutputCurrency}&base={exchange.InputCurrency}");
            var request = new RestRequest(Method.GET);
            IRestResponse response = await client.ExecuteAsync(request);


            var rateModel = JsonConvert.DeserializeObject<Rate>(response.Content);

            user.Currencies.First(x => x.CurrencyCode.Equals(exchange.InputCurrency)).Ammount -= exchange.Ammount;

            var rate = ParseStr(rateModel.rates.FirstOrDefault().Value);

            if (!user.Currencies.Any(x => x.CurrencyCode.Equals(exchange.OutputCurrency)))
                user.Currencies.Add(new Currency() { CurrencyCode = exchange.OutputCurrency, Ammount = Convert.ToDecimal((double)exchange.Ammount * rate) });
            else
                user.Currencies.First(x => x.CurrencyCode.Equals(exchange.OutputCurrency)).Ammount += Convert.ToDecimal((double)exchange.Ammount * rate);


            _context.Update(user);
            await _context.SaveChangesAsync();

            return Ok(response.Content);
        }

        /// <summary>
        /// Получение списка всех пользователей со кошельками
        /// </summary>
        /// <returns></returns>
        // GET: api/Wallet
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.OrderBy(x => x.Name).Include(c => c.Currencies).ToListAsync();
        }

        /// <summary>
        /// Получение конкретного пользователя и его  кошелька
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Wallet/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {

            var user = await _context.Users.Include(c => c.Currencies).Where(i => i.Id == id).FirstOrDefaultAsync();


            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        /// <summary>
        /// Снятие/Пополнение кошелька конкр пользователя
        /// </summary>
        /// <param name="id">ид пользователя</param>
        /// <param name="internalExchange">модель сняти/пополнения кошелька</param>
        /// <returns></returns>
        // PUT: api/Wallet/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, InternalExchange internalExchange)
        {
            if (!Enum.IsDefined(typeof(CurrencyEnum), internalExchange?.CurrencyCode))
            {
                return BadRequest();
            }

            var user = await _context.Users.Include(c => c.Currencies).FirstAsync(i => i.Id == id);

            if (user is null)
            {
                return NotFound();
            }

            if (internalExchange.Decrease
                 && internalExchange.Ammount > user.Currencies.First(x => x.CurrencyCode.Equals(internalExchange.CurrencyCode)).Ammount)
            {
                return BadRequest();
            }

            if (internalExchange.Decrease
                 && !user.Currencies.Any(x => x.CurrencyCode.Equals(internalExchange.CurrencyCode)))
            {
                return BadRequest();
            }


            try
            {
                if (internalExchange.Decrease)
                {

                    user.Currencies.First(x => x.CurrencyCode.Equals(internalExchange.CurrencyCode)).Ammount -= Convert.ToDecimal(internalExchange.Ammount);

                }
                else
                {
                    if (!user.Currencies.Any(x => x.CurrencyCode.Equals(internalExchange.CurrencyCode)))
                        user.Currencies.Add(new Currency() { CurrencyCode = internalExchange.CurrencyCode, Ammount = Convert.ToDecimal(internalExchange.Ammount) });
                    else
                        user.Currencies.First(x => x.CurrencyCode.Equals(internalExchange.CurrencyCode)).Ammount += Convert.ToDecimal(internalExchange.Ammount);
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok(user);
        }

        /// <summary>
        /// Добавление нового пользователя с кошельком
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        // POST: api/Wallet
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(UserInternal user)
        {
            if (user is null
                || !user.Currencies.Any())
            {
                return BadRequest();
            }

            var _currencies = new List<Currency>();
            foreach (var c in user.Currencies)
            {
                if (!Enum.IsDefined(typeof(CurrencyEnum), c.CurrencyCode)
                    || c.Ammount < 0)
                {
                    return BadRequest();
                }
                _currencies.Add(new Currency() { CurrencyCode = c.CurrencyCode, Ammount = c.Ammount });
            }
            var _user = new User()
            {
                Name = user.Name,
                Currencies = _currencies
            };

            _context.Users.Add(_user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = _user.Id }, user);
        }


        /// <summary>
        /// Удаление пользователя
        /// </summary>
        /// <param name="id">ид пользователя</param>
        /// <returns></returns>
        // DELETE: api/Wallet/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private double ParseStr(string str)
        {
            double value;
            return
                double.TryParse(str.Replace(",", "."), out value)
                ? value
                : double.Parse(str.Replace(".", ","));
        }
    }



}
