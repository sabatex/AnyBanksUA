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

public class PrivatUAConverter : BaseConvertor
{
    readonly StreamReader reader;
    string? line;
    int lineCounter = 0;
    string delimiter => "\n";



    const int BufferSize = 1024;

    public PrivatUAConverter(Stream stream, string fileExt, string accNumber = "") : base(stream, fileExt, accNumber)
    {
        reader = new StreamReader(_stream, new Encoding1251());
    }

    bool checkEnd(int position, ref char[] buffer)
    {
        foreach (var c in delimiter)
        {
            if (buffer[position++] != c) return false;
        }
        return true;
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
                throw new Exception(ErrorStrings.DetermineEndDelimiterForValue(valueName));
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

    string get1C8DateValue(ref int pos, string s, string valueName)
    {
        var ts = getValue(ref pos, s, valueName);
        if (!ts.TryDateTo1C8Date(out string result))
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

    DocumentSection GetDocument(string s, string AccCode)
    {
        var document = new DocumentSection();
        int pos = 0;
        string EDRPOU = getValue(ref pos, s, "ЄДРПОУ");
        if (EDRPOU == "ЄДРПОУ" || EDRPOU == "ЕГРПОУ") return null; // The header
        string MFO = getValue(ref pos, s, "МФО");
        string Account = getValue(ref pos, s, "Рахунок");
        string CurrencySymbolCode = getValue(ref pos, s, "Валюта");
        string DocummentNumber = getValue(ref pos, s, "Номер документа");
        string DateOperation = get1C8DateValue(ref pos, s, "Дата операції");
        string ClientMFO = getValue(ref pos, s, "МФО банка");
        string ClientBankName = getValue(ref pos, s, "Назва банка");
        string ClientAccount = getValue(ref pos, s, "Рахунок кореспондента");
        string ClientEDRPOU = getValue(ref pos, s, "ЄДРПОУ кореспондента");
        string ClientName = getValue(ref pos, s, "Кореспондент");
        decimal Summ = getDecimalDateValue(ref pos, s, "Сума");
        string Description = getValue(ref pos, s, "Призначення платежу");

        if (Summ < 0)
        {
            document.Сумма = -Summ;
            document.ПолучательОКПО = ClientEDRPOU;
            document.ПолучательМФО = ClientMFO;
            document.ПолучательСчет = ClientAccount;
            document.Получатель = ClientName;

            document.ПлательщикОКПО = EDRPOU;
            document.ПлательщикМФО = MFO;
            document.ПлательщикСчет = Account;
        }
        else
        {
            document.Сумма = Summ;
            document.ПолучательОКПО = EDRPOU;
            document.ПолучательМФО = MFO;
            document.ПолучательСчет = Account;

            document.ПлательщикОКПО = ClientEDRPOU;
            document.ПлательщикМФО = ClientMFO;
            document.ПлательщикСчет = ClientAccount;
            document.Плательщик = ClientName;
        }
        document.КодВалюты = CurrencySymbolCode;
        //Doc.ДатаПоступило = rec.DateOperation;
        document.ДокументИД = DocummentNumber;
        document.Дата = DateOperation;  //rec.DocumentDate;
        document.НазначениеПлатежа = Description;
        document.Номер = DocummentNumber;

        return document;
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
            result.EDRPOU = getValue(ref pos, line, "ЄДРПОУ");
            result.MFO = getValue(ref pos, line, "МФО");
            result.Account = getValue(ref pos, line, "Рахунок");
            result.CurrencySymbolCode = getValue(ref pos, line, "Валюта");
            result.DocummentNumber = getValue(ref pos, line, "Номер документа");
            result.DateOperation = get1C8DateValue(ref pos, line, "Дата операції");
            result.ClientMFO = getValue(ref pos, line, "МФО банка");
            result.ClientBankName = getValue(ref pos, line, "Назва банка");
            result.ClientAccount = getValue(ref pos, line, "Рахунок кореспондента");
            result.ClientEDRPOU = getValue(ref pos, line, "ЄДРПОУ кореспондента");
            result.ClientName = getValue(ref pos, line, "Кореспондент");
            result.Summ = getDecimalDateValue(ref pos, line, "Сума");
            result.Description = getValue(ref pos, line, "Призначення платежу");
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

            if (string.IsNullOrEmpty(line.Trim()) || line.StartsWith("ЄДРПОУ") || line.StartsWith("ЕГРПОУ"))
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
