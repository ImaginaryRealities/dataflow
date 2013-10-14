//-----------------------------------------------------------------------------
// <copyright file="MessageQueueFactory.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System.Messaging;

    /// <summary>
    /// Base class for a factory that is used to create MSMQ message queue
    /// objects at runtime.
    /// </summary>
    /// <remarks>
    /// The <c>MessageQueueFactory</c> class was implemented to support unit
    /// testing the MSMQ blocks. The <c>MessageQueueFactory</c> class is not
    /// intended to be used by consumers of the library.
    /// </remarks>
    public abstract class MessageQueueFactory
    {
        /// <summary>
        /// Creates a message queue object connected to the specified message
        /// queue and with the specified access mode.
        /// </summary>
        /// <param name="path">
        /// The path to the MSMQ queue to open.
        /// </param>
        /// <param name="accessMode">
        /// A <see cref="QueueAccessMode"/> object specifying the permissions
        /// that the new queue object should be granted.
        /// </param>
        /// <returns>
        /// A <see cref="MessageQueueBase"/> object.
        /// </returns>
        public abstract MessageQueueBase CreateMessageQueue(string path, QueueAccessMode accessMode);
    }
}
