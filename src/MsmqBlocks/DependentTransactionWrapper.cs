//-----------------------------------------------------------------------------
// <copyright file="DependentTransactionWrapper.cs" 
//            company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Transactions;

    internal class DependentTransactionWrapper : DependentTransactionBase
    {
        private readonly DependentTransaction dependentTransaction;

        public DependentTransactionWrapper(DependentTransaction dependentTransaction)
        {
            this.dependentTransaction = dependentTransaction;
        }

        protected override DependentTransaction DependentTransaction
        {
            get
            {
                return this.dependentTransaction;
            }
        }

        public override void Complete()
        {
            this.dependentTransaction.Complete();
        }

        public override void Rollback(Exception exception)
        {
            this.dependentTransaction.Rollback(exception);
        }
    }
}