//-----------------------------------------------------------------------------
// <copyright file="DependentTransactionBase.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Transactions;

    /// <summary>
    /// Abstracts the <see cref="DependentTransaction"/> class for unit
    /// testing the dataflow blocks.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class DependentTransactionBase
    {
        /// <summary>
        /// Gets the underlying transaction object.
        /// </summary>
        /// <remarks>
        /// Override this property in a derived class and return the actual
        /// <see cref="DependentTransaction"/> object.
        /// </remarks>
        /// <value>
        /// A <see cref="DependentTransaction"/> object.
        /// </value>
        protected abstract DependentTransaction DependentTransaction { get; }

        /// <summary>
        /// Converts a <see cref="DependentTransactionBase"/> object to a
        /// <see cref="DependentTransaction"/> object.
        /// </summary>
        /// <param name="wrapper">
        /// The <see cref="DependentTransactionBase"/> object to convert.
        /// </param>
        /// <returns>
        /// The <see cref="DependentTransaction"/> object.
        /// </returns>
        public static implicit operator DependentTransaction(DependentTransactionBase wrapper)
        {
            return wrapper.DependentTransaction;
        }

        /// <summary>
        /// Attempts to complete the dependent transaction.
        /// </summary>
        /// <seealso cref="System.Transactions.DependentTransaction.Complete"/>.
        public abstract void Complete();

        /// <summary>
        /// Rolls back (aborts) the transaction.
        /// </summary>
        /// <param name="exception">
        /// An explanation of why a rollback occurred.
        /// </param>
        public abstract void Rollback(Exception exception);
    }
}
