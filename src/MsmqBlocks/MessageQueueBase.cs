//-----------------------------------------------------------------------------
// <copyright file="MessageQueueBase.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Messaging;

    /// <summary>
    /// Abstracts the <see cref="MessageQueue"/> API for unit testing.
    /// </summary>
    /// <remarks>
    /// The <c>MessageQueueBase</c> class is used internally for unit testing
    /// the MSMQ dataflow blocks. This class is not intended to be used
    /// directly by consumers of the library.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public abstract class MessageQueueBase : IDisposable
    {
        private bool disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="MessageQueueBase"/> class. 
        /// </summary>
        ~MessageQueueBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Disposes of the message queue object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates an enumerator object for all of the messages in the queue.
        /// </summary>
        /// <returns>
        /// A <see cref="MessageEnumeratorBase"/> class.
        /// </returns>
        /// <seealso cref="MessageQueue.GetMessageEnumerator2"/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "MFC3: This is ok. It mirrors an API provided by the MessageQueue class.")]
        public abstract MessageEnumeratorBase GetMessageEnumerator();

        /// <summary>
        /// Receives the first message available in the queue referenced by
        /// the <see cref="MessageQueueBase"/> object and waits until either a
        /// message is available in the queue, or the timeout expires.
        /// </summary>
        /// <param name="timeout">
        /// The time to wait until a new message is available.
        /// </param>
        /// <returns>
        /// A <see cref="Message"/> object that contains the message received
        /// from the queue.
        /// </returns>
        /// <exception cref="MessageQueueException">
        /// A message did not arrive in the queue before the timeout expired.
        /// </exception>
        public abstract Message Receive(TimeSpan timeout);

        /// <summary>
        /// Sends an object to a non-transactional queue.
        /// </summary>
        /// <param name="obj">
        /// The object to send to the queue.
        /// </param>
        public abstract void Send(object obj);

        /// <summary>
        /// Override in a derived class to dispose of the message queue
        /// object.
        /// </summary>
        /// <param name="disposing">
        /// <c>True</c> if the object is being disposed, or <c>false</c>
        /// if the object is being garbage collected.
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
