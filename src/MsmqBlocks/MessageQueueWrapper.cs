//-----------------------------------------------------------------------------
// <copyright file="MessageQueueWrapper.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Messaging;

    internal sealed class MessageQueueWrapper : MessageQueueBase
    {
        private readonly MessageQueue messageQueue;

        private bool disposed;

        public MessageQueueWrapper(string path, QueueAccessMode accessMode)
        {
            this.messageQueue = new MessageQueue(path, accessMode);
        }

        public override IMessageFormatter Formatter
        {
            get
            {
                return this.messageQueue.Formatter;
            }

            set
            {
                this.messageQueue.Formatter = value;
            }
        }

        public override MessageEnumeratorBase GetMessageEnumerator()
        {
            return new MessageEnumeratorWrapper(this.messageQueue.GetMessageEnumerator2());
        }

        public override Message Receive(TimeSpan timeout)
        {
            return this.messageQueue.Receive(timeout);
        }

        public override Message Receive(TimeSpan timeout, MessageQueueTransactionType transactionType)
        {
            return this.messageQueue.Receive(timeout, transactionType);
        }

        public override void Send(object obj)
        {
            this.messageQueue.Send(obj);
        }

        public override void Send(object obj, MessageQueueTransactionType transactionType)
        {
            this.messageQueue.Send(obj, transactionType);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.disposed)
            {
                return;
            }

            this.messageQueue.Close();

            if (!disposing)
            {
                return;
            }

            this.messageQueue.Dispose();
            this.disposed = true;
        }
    }
}