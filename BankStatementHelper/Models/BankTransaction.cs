using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabatex.BankStatementHelper.Models
{
    public class BankTransaction
    {
        /// <summary>
        /// ЄДРПОУ 12345678
        /// </summary>
        public string? EDRPOU { get; set; }
        /// <summary>
        /// МФО 123456
        /// </summary>
        public string? MFO { get; set; }
        /// <summary>
        /// Рахунок UAxxxxxxxxxxxxxxxxxxxxxxx
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// Валюта UAH
        /// </summary>
        public string CurrencySymbolCode { get; set; }
        /// <summary>
        /// Номер документу 3363
        /// </summary>
        public string DocummentNumber { get; set; }
        /// <summary>
        /// Дата операції 11.11.2024
        /// </summary>
        public DateTime DateOperation { get; set; }
        /// <summary>
        /// МФО банку 123456
        /// </summary>
        public string ClientMFO { get; set; }
        /// <summary>
        /// Назва банку АТ "СЕНС БАНК"
        /// </summary>
        public string ClientBankName { get; set; } = string.Empty;
        /// <summary>
        /// Рахунок кореспондента UAxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        /// </summary>
        public string ClientAccount { get; set; }
        /// <summary>
        /// ЄДРПОУ кореспондента -8x-
        /// </summary>
        public string ClientEDRPOU { get; set; }
        /// <summary>
        /// Кореспондент   -Client Name-
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// Сума xxxx.xx  delimiter dot
        /// </summary>
        public decimal Summ { get; set; }
        /// <summary>
        /// Description any text
        /// </summary>
        public string Description { get; set; }


    }
}
