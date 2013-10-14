//-----------------------------------------------------------------------------
// <copyright file="MessageEnumeratorBase.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Messaging;

    /// <summary>
    /// Abstracts the <see cref="MessageEnumerator"/> API to support unit
    /// testing.
    /// </summary>
    /// <remarks>
    /// The <c>MessageEnumeratorBase</c> class is used to support unit testing
    /// the MSMQ dataflow blocks. The <c>MessageEnumeratorBase</c> class is not
    /// intended to be used by consumers of the library.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public abstract class MessageEnumeratorBase : IDisposable
    {
        private bool disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="MessageEnumeratorBase"/> class. 
        /// </summary>
        ~MessageEnumeratorBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Advances the enumerator to the next message in the queue. If the
        /// enumerator is positioned at the end of the queue, MoveNext waits
        /// until a message is available or the given timeout expires.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> value to wait for a message to become
        /// available.
        /// </param>
        /// <returns>
        /// <c>True</c> if the enumerator successfully advanced to the next
        /// message, or <c>false</c> if the enumerator reached the end of the
        /// queue and a message did not become available within the time
        /// specified by the <paramref name="timeout"/> parameter.
        /// </returns>
        public abstract bool MoveNext(TimeSpan timeout);

        /// <summary>
        /// Override in a derived class to dispose of the actual enumerator
        /// object.
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
