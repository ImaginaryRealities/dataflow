//-----------------------------------------------------------------------------
// <copyright file="InternalTransactionService.cs" 
//            company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System.Transactions;

    internal class InternalTransactionService : TransactionService
    {
        public override DependentTransactionBase CreateDependentTransaction(DependentCloneOption cloneOption)
        {
            var dependentTransaction = Transaction.Current.DependentClone(cloneOption);
            return new DependentTransactionWrapper(dependentTransaction);
        }

        public override TransactionScopeBase CreateTransactionScope()
        {
            return new TransactionScopeWrapper(new TransactionScope());
        }

        public override TransactionScopeBase CreateTransactionScope(DependentTransactionBase dependentTransaction)
        {
            return new TransactionScopeWrapper(new TransactionScope(dependentTransaction));
        }
    }
}