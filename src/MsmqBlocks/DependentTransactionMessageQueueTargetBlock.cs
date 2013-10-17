//-----------------------------------------------------------------------------
// <copyright file="DependentTransactionMessageQueueTargetBlock.cs"
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
    using System.Transactions;

    /// <summary>
    /// Dataflow block that will send a message to an MSMQ message queue as
    /// part of a transaction, and that will commit the transaction.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the body for the message.
    /// </typeparam>
    public class DependentTransactionMessageQueueTargetBlock<T> : ITargetBlock<Tuple<T, DependentTransactionBase>>
    {
        private readonly ITargetBlock<Tuple<T, DependentTransactionBase>> innerTargetBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentTransactionMessageQueueTargetBlock{T}"/> class.
        /// </summary>
        /// <param name="path">
        /// The path of the queue to send messages to.
        /// </param>
        public DependentTransactionMessageQueueTargetBlock(string path)
            : this(new InternalTransactionService(), new InternalMessageQueueFactory(), path)
        {
        }

        internal DependentTransactionMessageQueueTargetBlock(
            TransactionService transactionService,
            MessageQueueFactory messageQueueFactory,
            string path)
        {
            this.innerTargetBlock = new ActionBlock<Tuple<T, DependentTransactionBase>>(
                m =>
                    {
                        try
                        {
                            using (var transactionScope = transactionService.CreateTransactionScope(m.Item2))
                            {
                                var messageQueue = messageQueueFactory.CreateMessageQueue(path, QueueAccessMode.Send);
                                using (messageQueue)
                                {
                                    messageQueue.Send(m.Item1, MessageQueueTransactionType.Automatic);
                                }

                                transactionScope.Complete();
                            }

                            m.Item2.Complete();
                        }
                        catch (Exception ex)
                        {
                            m.Item2.Rollback(ex);
                            throw;
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

        DataflowMessageStatus ITargetBlock<Tuple<T, DependentTransactionBase>>.OfferMessage(
            DataflowMessageHeader messageHeader,
            Tuple<T, DependentTransactionBase> messageValue,
            ISourceBlock<Tuple<T, DependentTransactionBase>> source,
            bool consumeToAccept)
        {
            return this.innerTargetBlock.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }
    }
}
