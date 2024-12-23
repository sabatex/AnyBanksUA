using Sabatex.Extensions.Text;
using Sabatex.BankStatementHelper;
using Sabatex.BankStatementHelper.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace sabatex.Tests.BankHelper
{
    public class BankStreamConverterTests
    {
        const string testFilePath = @"C:\Users\serhi\OneDrive\DataBases\BankHelper";

        [Theory]
        [InlineData("26005034006185", testFilePath + "/PrimaBankSK.csv", EBankType.PrimaBankSK, 1,"PrimaBankSK")]
        [InlineData("26005034006185", testFilePath + "/OtpBankSK.csv", EBankType.OtpBankSK, 1,"OtpBankSK")]
        [InlineData("UA923052990000026008050334389", testFilePath + "/Privat24.csv", EBankType.PrivatUA,21, "Privat24")]
        [InlineData("26005034006185", testFilePath + "/iBankUA.csv", EBankType.iBankUA,1, "IBankUA")]
        [InlineData("", testFilePath+ "/OTPBank_210211.zip", EBankType.iFobs,280, "iFobsXML")]
        [InlineData("", testFilePath + "/Львів/CB_to_1C_20210101-20210309.zip", EBankType.iFobs,1, "iFobsXML")]
        [InlineData("26005034006185", testFilePath + "/iFobsEximBank.dat", EBankType.iFobs,1, "EXIMBank")]
        [InlineData("", testFilePath + "/iFobsEximBank.dat", EBankType.iFobs,1, "EXIMBank without accouunt")]
        [InlineData("26005034006186", testFilePath + "/iFobsExim191004.dat", EBankType.iFobs,1, "EXIMBank wrong account")]
        [InlineData("", testFilePath + @"\GASBank\account_statement_21122021-21122021_221220211532.csv", EBankType.UkrGazBank, 13,"GazBank with account")]
        [InlineData("", testFilePath + "/ощадбанк.csv", EBankType.Oschad, 11,"ощадбанк")]
        public async void ConvertTo1CFormatNew(string accNumber, string fileName, EBankType bankType, int lines,string name)
        {
            using (Stream stream = File.OpenRead(fileName))
            {
                        var converter = ClientBankTo1CFormatConversion.GetConvertor(bankType);
                        Assert.NotNull(converter);
                        await converter.ImportFromFileAsync(stream,Path.GetExtension(fileName) ,accNumber);
                        Assert.True(converter.Errors.Count ==0);
                        Assert.Equal(converter.Documents.Count,lines);


            }
 
        }
        [Theory]
        [InlineData(testFilePath + "/Privat24.csv", EBankType.PrivatUA, 21,12, "Privat24")]
        [InlineData(testFilePath + "/Sense-Bank.csv", EBankType.SensBank, 32, -41871.79, "SensBank")]
        public async void ParseBankStatement(string fileName, EBankType bankType, int lines,decimal sum, string name)
        {
            using (Stream stream = File.OpenRead(fileName))
            {
                string destination = string.Empty;
                await using (var collection = ConvertorExtensions.GetConvertor(bankType, stream, Path.GetExtension(fileName)))
                {
                    var result = new List<BankTransaction>();
                    await foreach (var item in collection) 
                    {
                        result.Add(item);
                    }
                    Assert.Equal(result.Count, lines);
                    Assert.Equal(result.Sum(s=>s.Summ), sum);

                    var s = result.GetAsSensBankCSV();
                    destination = result.GetAsIBankUACSV();

                  }
                //back test privat
                //Encoding win1251 = Encoding.GetEncoding("windows-1251");
                //byte[] win1251Bytes = Encoding.Convert(Encoding.UTF8, win1251, utf8Bytes);
                //var mstream = new MemoryStream((new Encoding1251()).GetBytes(destination));
                //await using (var collection = ConvertorExtensions.GetConvertor(EBankType.iBankUA, mstream, Path.GetExtension(fileName)))
                //{
                //    var result = new List<BankTransaction>();
                //    await foreach (var item in collection)
                //    {
                //        result.Add(item);
                //    }
                //    Assert.Equal(result.Count, lines);
                //    Assert.Equal(result.Sum(s => s.Summ), sum);

                //}






            }

        }

    }
}