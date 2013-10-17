//-----------------------------------------------------------------------------
// <copyright file="TransactionalMessageQueueTargetBlock.cs" 
//            company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Messaging;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    /// <summary>
    /// Dataflow target block that will send messages to a transactional MSMQ
    /// queue.
    /// </summary>
    /// <typeparam name="T">
    /// The type for the body of the message.
    /// </typeparam>
    public sealed class TransactionalMessageQueueTargetBlock<T> : ITargetBlock<T>
    {
        private readonly ITargetBlock<T> innerTargetBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalMessageQueueTargetBlock{T}"/> class.
        /// </summary>
        /// <param name="path">
        /// The path of the message queue to send messages to.
        /// </param>
        public TransactionalMessageQueueTargetBlock(string path)
            : this(new InternalTransactionService(), new InternalMessageQueueFactory(), path)
        {
        }

        internal TransactionalMessageQueueTargetBlock(
            TransactionService transactionService,
            MessageQueueFactory messageQueueFactory,
            string path)
        {
            this.innerTargetBlock = new ActionBlock<T>(
                m =>
                    {
                        using (var transactionScope = transactionService.CreateTransactionScope())
                        {
                            var messageQueue = messageQueueFactory.CreateMessageQueue(path, QueueAccessMode.Send);
                            using (messageQueue)
                            {
                                messageQueue.Send(m, MessageQueueTransactionType.Automatic);
                            }

                            transactionScope.Complete();
                        }
                    });
        }

        /// <summary>
        /// Gets a <see cref="Task"/> object that represents the asynchronous
        /// operation and completion of the dataflow block.
        /// </summary>
        /// <value>
        /// A <see cref="Task"/> object.
        /// </value>
        public Task Completion
        {
            get
            {
                return this.innerTargetBlock.Completion;
            }
        }

        /// <summary>
        /// Signals to the dataflow block that it should not accept nor
        /// produce any more messages nor consume any more postponed messages.
        /// </summary>
        public void Complete()
        {
            this.innerTargetBlock.Complete();
        }

        /// <summary>
        /// Causes the dataflow block to complete in a faulted state.
        /// </summary>
        /// <param name="exception">
        /// The exception that caused the fault.
        /// </param>
        public void Fault(Exception exception)
        {
            this.innerTargetBlock.Fault(exception);
        }

        DataflowMessageStatus ITargetBlock<T>.OfferMessage(
            DataflowMessageHeader messageHeader,
            T messageValue,
            ISourceBlock<T> source,
            bool consumeToAccept)
        {
            return this.innerTargetBlock.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }
    }
}
