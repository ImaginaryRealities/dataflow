//-----------------------------------------------------------------------------
// <copyright file="TransactionScopeWrapper.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System.Transactions;

    internal class TransactionScopeWrapper : TransactionScopeBase
    {
        private readonly TransactionScope transactionScope;

        private bool disposed;

        public TransactionScopeWrapper(TransactionScope transactionScope)
        {
            this.transactionScope = transactionScope;
        }

        public override void Complete()
        {
            this.transactionScope.Complete();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.disposed || !disposing)
            {
                return;
            }

            this.transactionScope.Dispose();
            this.disposed = true;
        }
    }
}