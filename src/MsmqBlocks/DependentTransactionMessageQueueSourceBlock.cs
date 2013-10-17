//-----------------------------------------------------------------------------
// <copyright file="DependentTransactionMessageQueueSourceBlock.cs"
//            company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Messaging;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using System.Transactions;

    /// <summary>
    /// Dataflow block that will receive messages from a transactional MSMQ
    /// queue and will output the messages and the transactions.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the message body.
    /// </typeparam>
    public sealed class DependentTransactionMessageQueueSourceBlock<T> :
        IReceivableSourceBlock<Tuple<T, DependentTransactionBase>>
    {
        private readonly BufferBlock<Tuple<T, DependentTransactionBase>> bufferBlock;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private readonly IReceivableSourceBlock<Tuple<T, DependentTransactionBase>> innerSourceBlock;

        private readonly MessageQueueFactory messageQueueFactory;

        private readonly string path;

        private readonly Thread receiveThread;

        private readonly TransactionService transactionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentTransactionMessageQueueSourceBlock{T}"/> class.
        /// </summary>
        /// <param name="path">
        /// The path of the message queue to receive messages from.
        /// </param>
        public DependentTransactionMessageQueueSourceBlock(string path)
            : this(new InternalMessageQueueFactory(), new InternalTransactionService(), path)
        {
        }

        internal DependentTransactionMessageQueueSourceBlock(
            MessageQueueFactory messageQueueFactory,
            TransactionService transactionService,
            string path)
        {
            this.bufferBlock = new BufferBlock<Tuple<T, DependentTransactionBase>>();
            this.innerSourceBlock = this.bufferBlock;
            this.messageQueueFactory = messageQueueFactory;
            this.transactionService = transactionService;
            this.path = path;
            this.receiveThread = new Thread(this.ReceiveMessages)
                {
                    IsBackground = true,
                    Name =
                        string.Format(CultureInfo.CurrentCulture, "DependentTransactionMessageQueueSourceBlock: {0}", path)
                };
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
                return this.innerSourceBlock.Completion;
            }
        }

        /// <summary>
        /// Signals to the dataflow block that it should not accept nor
        /// produce any more messages nor consume any more postponed messages.
        /// </summary>
        public void Complete()
        {
            this.cancellationTokenSource.Cancel();
            this.receiveThread.Join();
            this.innerSourceBlock.Complete();
            this.cancellationTokenSource.Dispose();
        }

        /// <summary>
        /// Causes the dataflow block to complete in a faulted state.
        /// </summary>
        /// <param name="exception">
        /// The exception that caused the fault.
        /// </param>
        public void Fault(Exception exception)
        {
            this.cancellationTokenSource.Cancel();
            this.receiveThread.Join();
            this.innerSourceBlock.Fault(exception);
            this.cancellationTokenSource.Dispose();
        }

        /// <summary>
        /// Links the dataflow block to a target block.
        /// </summary>
        /// <param name="target">
        /// The target block to link the dataflow block to.
        /// </param>
        /// <param name="linkOptions">
        /// Options for the link.
        /// </param>
        /// <returns>
        /// Returns the subscription object for the link.
        /// </returns>
        public IDisposable LinkTo(ITargetBlock<Tuple<T, DependentTransactionBase>> target, DataflowLinkOptions linkOptions)
        {
            return this.innerSourceBlock.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Begins receiving messages from the MSMQ message queue and
        /// outputting the messages to the pipeline.
        /// </summary>
        public void Start()
        {
            this.receiveThread.Start();
        }

        bool IReceivableSourceBlock<Tuple<T, DependentTransactionBase>>.TryReceive(
            Predicate<Tuple<T, DependentTransactionBase>> filter,
            out Tuple<T, DependentTransactionBase> item)
        {
            return this.innerSourceBlock.TryReceive(filter, out item);
        }

        bool IReceivableSourceBlock<Tuple<T, DependentTransactionBase>>.TryReceiveAll(
            out IList<Tuple<T, DependentTransactionBase>> items)
        {
            return this.innerSourceBlock.TryReceiveAll(out items);
        }

        Tuple<T, DependentTransactionBase> ISourceBlock<Tuple<T, DependentTransactionBase>>.ConsumeMessage(
            DataflowMessageHeader messageHeader,
            ITargetBlock<Tuple<T, DependentTransactionBase>> target,
            out bool messageConsumed)
        {
            return this.innerSourceBlock.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        void ISourceBlock<Tuple<T, DependentTransactionBase>>.ReleaseReservation(
            DataflowMessageHeader messageHeader,
            ITargetBlock<Tuple<T, DependentTransactionBase>> target)
        {
            this.innerSourceBlock.ReleaseReservation(messageHeader, target);
        }

        bool ISourceBlock<Tuple<T, DependentTransactionBase>>.ReserveMessage(
            DataflowMessageHeader messageHeader,
            ITargetBlock<Tuple<T, DependentTransactionBase>> target)
        {
            return this.innerSourceBlock.ReserveMessage(messageHeader, target);
        }

        private void ReceiveMessages()
        {
            try
            {
                var timeout = TimeSpan.FromSeconds(10.0);
                while (!this.cancellationTokenSource.IsCancellationRequested)
                {
                    using (var transactionScope = this.transactionService.CreateTransactionScope())
                    {
                        var messageQueue = this.messageQueueFactory.CreateMessageQueue(
                            this.path,
                            QueueAccessMode.Receive);
                        using (messageQueue)
                        {
                            messageQueue.Formatter = new XmlMessageFormatter(new[] { typeof(T) });
                            using (var enumerator = messageQueue.GetMessageEnumerator())
                            {
                                if (!enumerator.MoveNext(timeout))
                                {
                                    continue;
                                }

                                var message = messageQueue.Receive(timeout, MessageQueueTransactionType.Automatic);
                                var dependentTransaction =
                                    this.transactionService.CreateDependentTransaction(
                                        DependentCloneOption.BlockCommitUntilComplete);
                                this.bufferBlock.Post(Tuple.Create((T)message.Body, dependentTransaction));
                                message.Dispose();
                                transactionScope.Complete();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.innerSourceBlock.Fault(ex);
            }
        }
    }
}
