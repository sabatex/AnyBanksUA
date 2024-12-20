using sabatex.Extensions.ClassExtensions;
using sabatex.Extensions.Text;
using Sabatex.BankStatementHelper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;



namespace Sabatex.BankStatementHelper;

public class SensBankUAConverter : BaseConvertor
{
    readonly StreamReader reader;
    string? line;
    int lineCounter = 0;
 
    public SensBankUAConverter(Stream stream, string fileExt, string accNumber = "") : base(stream, fileExt, accNumber)
    {
        reader = new StreamReader(_stream, new Encoding1251());
    }

    string getValue(ref int pos, string s, string valueName)
    {
        if (pos == s.Length)
            throw new Exception(ErrorStrings.TryGetValueFromEndString(valueName));

        if (s[pos] == '\n' || s[pos] == '\r')
            throw new Exception(ErrorStrings.TryGetValueFromEndString(valueName));

        int start = pos;
        if (s[pos] == '"')
        {
            pos++;
            start = pos;
        }
        else
        {
            pos = s.IndexOf(';', start);
            if (pos == -1)
            {
                // check end
                pos =s.Length;
                return s.Substring(start);
            }
            return s.Substring(start, pos++ - start);
        }

        var result = new StringBuilder();
        // special string
        while (pos < s.Length)
        {
            switch (s[pos])
            {
                case '"':
                    pos++;
                    if (pos == s.Length)
                        throw new Exception(ErrorStrings.StringEndedBeforeReadValue(valueName));

                    if (s[pos] == ';')
                    {
                        pos++;
                        return result.ToString();
                    }

                    if (s[pos] == '"')
                    {
                        result.Append('"');
                        pos++;
                        break;
                    }
                    throw new Exception(ErrorStrings.StringFormatErrorForValue(valueName));
                default:
                    result.Append(s[pos]);
                    pos++;
                    break;

            }
        }
        throw new Exception(ErrorStrings.DetermineEndDelimiterForValue(valueName));
    }

    DateTime getDateValue(ref int pos, string s, string valueName)
    {
        var ts = getValue(ref pos, s, valueName);
        if (!DateTime.TryParse(ts, out DateTime result))
            throw new Exception(ErrorStrings.ConvertDataTo1C8FormatForField("'Дата операції'", ts));
        else
            return result;
    }

    decimal getDecimalDateValue(ref int pos, string s, string valueName)
    {
        var ts = getValue(ref pos, s, valueName);
        if (ts.Length == 0)
        {
            return 0;
        }
        else
        {
            if (!ts.TryToDecimal(out decimal result))
                throw new Exception(ErrorStrings.DoubleParse(ts));
            else
                return result;
        }
    }

    public override BankTransaction Current 
    {
        get
        {
            if (line == null) 
            {
                throw new Exception("Try parse null line");
            }
            var result = new BankTransaction();
            int pos = 0;
            var temp = getValue(ref pos, line, "Наш рахунок (1)");
            result.Account = getValue(ref pos, line, "Наш IBAN (2)");
            var operation = getValue(ref pos, line, "Операція (3)");
            temp = getValue(ref pos, line, "рахунок (4)");
            result.ClientAccount = getValue(ref pos, line, "Рахунок кореспондента IBAN (5)");
            result.ClientMFO = getValue(ref pos, line, "МФО банку контрагента (6)");
            result.ClientName = getValue(ref pos, line, "Найменування контрагента (7)");
            result.ClientEDRPOU = getValue(ref pos, line, "ЄДРПОУ кореспондента (8)");
            result.Description = getValue(ref pos, line, "Призначення платежу (9)");
            result.DateOperation = getDateValue(ref pos, line, "Дата проведення (10)");
            result.DocummentNumber = getValue(ref pos, line, "Номер документа (11)");
            decimal sum = getDecimalDateValue(ref pos, line, "Сума (12)");
            result.CurrencySymbolCode = getValue(ref pos, line, "Валюта");
            temp = getValue(ref pos, line, "Час проведення (13)");
            temp = getValue(ref pos, line, "Дата документа (14)");
            temp = getValue(ref pos, line, "Дата архівування (15)");
            result.EDRPOU = getValue(ref pos, line, "ЄДРПОУ - Ід.код (16)");
            temp = getValue(ref pos, line, "Найменування (17)");
            result.MFO = getValue(ref pos, line, "МФО (18)");
            if (operation.ToLower() == "Кредит".ToLower())
                result.Summ = -sum;
            else
                result.Summ = sum;


            return result;
        }
    }
    public override async ValueTask DisposeAsync()
    {
        reader?.Dispose();
        await Task.Yield();
    }

    public override IAsyncEnumerator<BankTransaction> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return this;
    }

    public override async ValueTask<bool> MoveNextAsync()
    {
        line = await reader.ReadLineAsync();
        // end stream
        if (line == null)
        {
            return false;
        }

        // search start line
        while (lineCounter == 0)
        {
            // end stream
            if (line == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(line.Trim()) || line.StartsWith("Наш рахунок;Наш IBAN;Операція;"))
            {
                line = await reader.ReadLineAsync();
                continue;
            }

            break;
        }
        lineCounter++;
        return true;
  
    }
}
