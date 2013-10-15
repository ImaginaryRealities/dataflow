//-----------------------------------------------------------------------------
// <copyright file="TransactionScopeBase.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Transactions;

    /// <summary>
    /// Abstracts the <see cref="TransactionScope"/> API for unit testing.
    /// </summary>
    /// <remarks>
    /// The <c>TransactionScopeBase</c> class wraps the 
    /// <see cref="TransactionScope"/> API to support unit testing. It is not
    /// intended to be used by consumers of the library.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public abstract class TransactionScopeBase : IDisposable
    {
        private bool disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="TransactionScopeBase"/> class.
        /// </summary>
        ~TransactionScopeBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Indicates that all operations within the scope are completed
        /// successfully.
        /// </summary>
        /// <seealso cref="TransactionScope.Complete"/>
        public abstract void Complete();

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Override in a derived class to dispose of the underlying
        /// <see cref="TransactionScope"/> object.
        /// </summary>
        /// <param name="disposing">
        /// <c>True</c> if the object is being disposed, or <c>false</c> if
        /// the object is being garbage collected.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed || !disposing)
            {
                return;
            }

            this.disposed = true;
        }
    }
}
