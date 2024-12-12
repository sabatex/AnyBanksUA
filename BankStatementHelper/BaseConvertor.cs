using Sabatex.BankStatementHelper.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Sabatex.BankStatementHelper
{
    public abstract class BaseConvertor : IAsyncEnumerable<BankTransaction>, IAsyncEnumerator<BankTransaction>
    {
        protected readonly Stream _stream;
        protected BaseConvertor(Stream stream, string fileExt, string accNumber = "")
        {
            _stream = stream;
        }

        public abstract BankTransaction Current { get; }

 

        public abstract ValueTask DisposeAsync();
        public abstract IAsyncEnumerator<BankTransaction> GetAsyncEnumerator(CancellationToken cancellationToken = default);
        public abstract ValueTask<bool> MoveNextAsync();
    }
}
