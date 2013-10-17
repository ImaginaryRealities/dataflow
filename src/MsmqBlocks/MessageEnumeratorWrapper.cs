//-----------------------------------------------------------------------------
// <copyright file="MessageEnumeratorWrapper.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System;
    using System.Messaging;

    internal sealed class MessageEnumeratorWrapper : MessageEnumeratorBase
    {
        private readonly MessageEnumerator messageEnumerator;

        private bool disposed;

        public MessageEnumeratorWrapper(MessageEnumerator messageEnumerator)
        {
            this.messageEnumerator = messageEnumerator;
        }

        public override bool MoveNext(TimeSpan timeout)
        {
            return this.messageEnumerator.MoveNext(timeout);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.disposed)
            {
                return;
            }

            this.messageEnumerator.Close();

            if (!disposing)
            {
                return;
            }

            this.messageEnumerator.Dispose();
            this.disposed = true;
        }
    }
}