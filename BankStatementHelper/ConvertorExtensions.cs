using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                default:
                    throw new NotImplementedException();

            }
        }
    }
}
