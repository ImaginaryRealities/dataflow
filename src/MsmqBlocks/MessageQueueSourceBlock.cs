//-----------------------------------------------------------------------------
// <copyright file="MessageQueueSourceBlock.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Messaging;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    using PostSharp.Patterns.Diagnostics;

    /// <summary>
    /// Dataflow block that will output messages that are received from a
    /// non-transactional MSMQ queue.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the body of the messages that are received from the MSMQ
    /// queue.
    /// </typeparam>
    [Log]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "MFC3: The disposable objects are disposed in the Complete and Fault methods.")]
    public sealed class MessageQueueSourceBlock<T> : IReceivableSourceBlock<T>
    {
        private readonly BufferBlock<T> bufferBlock;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private readonly IReceivableSourceBlock<T> innerBlock;

        private readonly MessageQueueFactory messageQueueFactory;

        private readonly string path;

        private readonly Thread receiveThread;

        internal MessageQueueSourceBlock(MessageQueueFactory messageQueueFactory, string path)
        {
            this.messageQueueFactory = messageQueueFactory;
            this.path = path;
            this.receiveThread = new Thread(this.ReceiveMessages)
                {
                    IsBackground = true,
                    Name = string.Format(CultureInfo.CurrentCulture, "MessageQueueSourceBlock: {0}", path)
                };
            this.bufferBlock = new BufferBlock<T>();
            this.innerBlock = this.bufferBlock;
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
                return this.innerBlock.Completion;
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
            this.innerBlock.Complete();
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
            this.innerBlock.Fault(exception);
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
        public IDisposable LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions)
        {
            return this.innerBlock.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Begins receiving messages from the MSMQ message queue and
        /// outputting the messages to the pipeline.
        /// </summary>
        public void Start()
        {
            this.receiveThread.Start();
        }

        /// <summary>
        /// Outputs the path to the message queue that the block receives
        /// messages from.
        /// </summary>
        /// <returns>
        /// The path to the message queue.
        /// </returns>
        public override string ToString()
        {
            return this.path ?? base.ToString();
        }

        bool IReceivableSourceBlock<T>.TryReceive(Predicate<T> filter, out T item)
        {
            return this.innerBlock.TryReceive(filter, out item);
        }

        bool IReceivableSourceBlock<T>.TryReceiveAll(out IList<T> items)
        {
            return this.innerBlock.TryReceiveAll(out items);
        }

        T ISourceBlock<T>.ConsumeMessage(
            DataflowMessageHeader messageHeader,
            ITargetBlock<T> target,
            out bool messageConsumed)
        {
            return this.innerBlock.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        void ISourceBlock<T>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        {
            this.innerBlock.ReleaseReservation(messageHeader, target);
        }

        bool ISourceBlock<T>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        {
            return this.innerBlock.ReserveMessage(messageHeader, target);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification =
                "MFC3: ReceiveMessages runs in its own thread. I want to catch any exception and report it as a fault.")]
        private void ReceiveMessages()
        {
            try
            {
                var timeout = TimeSpan.FromSeconds(10.0);
                using (
                    var messageQueue = this.messageQueueFactory.CreateMessageQueue(this.path, QueueAccessMode.Receive))
                using (var enumerator = messageQueue.GetMessageEnumerator())
                {
                    while (!this.cancellationTokenSource.IsCancellationRequested)
                    {
                        if (!enumerator.MoveNext(timeout))
                        {
                            continue;
                        }

                        using (var message = messageQueue.Receive(timeout))
                        {
                            this.bufferBlock.Post((T)message.Body);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.innerBlock.Fault(ex);
                this.cancellationTokenSource.Dispose();
            }
        }
    }
}
