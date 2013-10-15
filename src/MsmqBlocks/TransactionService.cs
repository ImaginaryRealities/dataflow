//-----------------------------------------------------------------------------
// <copyright file="TransactionService.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Transactions;

    /// <summary>
    /// Factory class that is used to create <see cref="TransactionScope"/>
    /// objects.
    /// </summary>
    /// <remarks>
    /// The <c>TransactionService</c> class is used to abstract the
    /// process of creating <see cref="TransactionScope"/> objects from the
    /// code. This class is used to support unit testing the dataflow blocks.
    /// It is not intended to be used by consumers of this library.
    /// </remarks>
    public abstract class TransactionService
    {
        /// <summary>
        /// Creates a dependent clone of the current transaction.
        /// </summary>
        /// <param name="cloneOption">
        /// A <see cref="DependentCloneOption"/> value that controls what kind
        /// of dependent transaction to create.
        /// </param>
        /// <returns>
        /// A <see cref="DependentTransactionBase"/> object.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// There is not an active transaction.
        /// </exception>
        public abstract DependentTransactionBase CreateDependentTransaction(DependentCloneOption cloneOption);

        /// <summary>
        /// Creates a <see cref="TransactionScope"/> object.
        /// </summary>
        /// <returns>
        /// A <see cref="TransactionScopeBase"/> object.
        /// </returns>
        public abstract TransactionScopeBase CreateTransactionScope();
    }
}
