using Sabatex.BankStatementHelper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sabatex.BankStatementHelper
{
    public static class ConvertorExtensions
    {
        public static BaseConvertor GetConvertor(EBankType bankType, Stream stream, string fileExtension)
        {
            switch (bankType)
            {
                case EBankType.PrivatUA: return new PrivatUAConverter(stream, fileExtension);
                case EBankType.SensBank: return new SensBankUAConverter(stream, fileExtension);
                default:
                    throw new NotImplementedException();

            }
        }
    
        public static string GetAsPrivatCSV(this IEnumerable<BankTransaction> bankTransactions)
        {
            var result = new StringBuilder();
            result.AppendLine("ЄДРПОУ;МФО;Рахунок;Валюта;Номер документу;Дата операції;МФО банку;Назва банку;Рахунок кореспондента;ЄДРПОУ кореспондента;Кореспондент;Сума;Призначення платежу;");
            foreach (var transaction in bankTransactions)
            {
                result.AppendLine($"{transaction.EDRPOU};{transaction.MFO};{transaction.Account};{transaction.CurrencySymbolCode};{transaction.DocummentNumber};{transaction.DateOperation.ToString("dd.MM.yyyy")};{transaction.ClientMFO};{transaction.ClientBankName.Replace(';',':')};{transaction.ClientAccount};{transaction.ClientEDRPOU};{transaction.ClientName.Replace(';',':')};{transaction.Summ.ToString().Replace(',','.')};{transaction.Description.Replace(';', ':')}");
            }
            return result.ToString();
        }
        public static string GetAsIBankUACSV(this IEnumerable<BankTransaction> bankTransactions)
        {
            string iBankData(DateTime date) => date.ToString("dd.MM.yyyy hh:mm");
            string debet(decimal summ) => summ >= 0 ? summ.ToString("0.00") : "";
            string credit(decimal summ) => summ < 0 ? (-summ).ToString("0.00") : "";
            string format(string s)
            {
                if (s.Contains(';') || s.Contains('"'))
                {
                    var result = new StringBuilder();
                    result.Append('"');
                    foreach (var c in s)
                    {
                        result.Append(c);
                        if (c== '"') result.Append('"');
                    }
                    result.Append('"');
                }
                return s;
            }
            var result = new StringBuilder();
            //result.AppendLine("ЄДРПОУ;МФО;Рахунок;Валюта;Дата операції;Код операції;МФО банка;Назва банка;Рахунок кореспондента;ЄДРПОУ кореспондента;Кореспондент;Документ;Дата документа;Дебет;Кредит;Призначення платежу;Гривневе покриття");
            result.AppendLine("ЄДРПОУ;Код ID НБУ;Рахунок;Валюта;Дата операції;Код операції;Код ID НБУ надавача;Надавач платіжних послуг;Рахунок кореспондента;ЄДРПОУ кореспондента;Кореспондент;Номер документа;Дата документа;Дебет;Кредит;Призначення платежу;Гривневе покриття");
            foreach (var transaction in bankTransactions)
            {
                result.Append($"{transaction.EDRPOU};");                   //ЄДРПОУ
                result.Append($"{transaction.MFO};");                      //МФО  - Код ID НБУ
                result.Append($"{transaction.Account};");                  //Рахунок
                result.Append($"{transaction.CurrencySymbolCode};");       //Валюта
                result.Append($"{iBankData(transaction.DateOperation)};"); //Дата операції
                result.Append($";");                                       //Код операції
                result.Append($"{transaction.ClientMFO};");                //МФО банка - Код ID НБУ надавача
                result.Append($"{format(transaction.ClientBankName)};");   //Надавач платіжних послуг 
                result.Append($"{transaction.ClientAccount};");            //Рахунок кореспондента
                result.Append($"{transaction.ClientEDRPOU};");             //ЄДРПОУ кореспондента
                result.Append($"{format(transaction.ClientName)};");       //Кореспондент
                result.Append($"{transaction.DocummentNumber};");          //Номер документа
                result.Append($"{iBankData(transaction.DateOperation)};"); //Дата документа                                                  
                result.Append($"{debet(transaction.Summ)};");              //Дебет 
                result.Append($"{credit(transaction.Summ)};");             //Кредит
                result.Append($"{format(transaction.Description)};");      //Призначення платежу
                result.Append($"0.00;");//Гривневе покриття
                result.Append("\r\n"); 
            }
            return result.ToString();
        }
        public static string GetAsSensBankCSV(this IEnumerable<BankTransaction> bankTransactions)
        {
             string format(string s)
            {
                if (s.Contains(';') || s.Contains('"'))
                {
                    var result = new StringBuilder();
                    result.Append('"');
                    foreach (var c in s)
                    {
                        result.Append(c);
                        if (c == '"') result.Append('"');
                    }
                    result.Append('"');
                }
                return s;
            }
            string accountFronIBAN(string s) => s.Substring(Math.Max(0, s.Length - 14));
            string operation(BankTransaction transaction)=>transaction.Summ>=0? "Дебет" : "Кредит";
            string time(DateTime date) => date.ToString("hh:mm:ss");
            string date(DateTime date) => date.ToString("dd.MM.yyyy");
            var result = new StringBuilder();
            result.AppendLine("Наш рахунок;Наш IBAN;Операція;Рахунок;IBAN;МФО банку контрагента;Найменування контрагента;Код контрагента;Призначення платежу;Дата проведення;Номер документа;Сума;Валюта;Час проведення;Дата документа;Дата архівування;Ід.код;Найменування;МФО\r\n");
            foreach (var transaction in bankTransactions)
            {
                result.Append($"{accountFronIBAN(transaction.Account)};"); //Наш рахунок
                result.Append($"{transaction.Account};");                  //Наш IBAN
                result.Append($"{operation(transaction)};");               //Операція
                result.Append($"{accountFronIBAN(transaction.ClientAccount)};"); //рахунок кліэнта
                result.Append($"{transaction.ClientAccount};");            //Рахунок кореспондента
                result.Append($"{transaction.ClientMFO};");                //МФО банка - Код ID НБУ надавача
                result.Append($"{format(transaction.ClientName)};");       //Кореспондент
                result.Append($"{transaction.ClientEDRPOU};");             //ЄДРПОУ кореспондента
                result.Append($"{format(transaction.Description)};");      //Призначення платежу
                result.Append($"{date(transaction.DateOperation)};");      //Дата документа                      
                result.Append($"{transaction.DocummentNumber};");          //Номер документа
                result.Append($"{Math.Abs(transaction.Summ)};");           //Сума
                result.Append($"{transaction.CurrencySymbolCode};");       //Валюта
                result.Append($"{time(transaction.DateOperation)};");       //Час проведення
                result.Append($"{date(transaction.DateOperation)};");       //Дата документа
                result.Append($"{date(transaction.DateOperation)};");       //Дата архівування
                result.Append($"{transaction.EDRPOU};");                    //ЄДРПОУ    
                result.Append($";");                                         //Name
                result.Append($"{transaction.MFO}");                      //МФО  - Код ID НБУ
            }
            return result.ToString();
        }


    }
}
