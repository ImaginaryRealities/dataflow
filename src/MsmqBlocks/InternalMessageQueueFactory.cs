//-----------------------------------------------------------------------------
// <copyright file="InternalMessageQueueFactory.cs" 
//            company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq
{
    using System.Messaging;

    internal sealed class InternalMessageQueueFactory : MessageQueueFactory
    {
        public override MessageQueueBase CreateMessageQueue(string path, QueueAccessMode accessMode)
        {
            return new MessageQueueWrapper(path, accessMode);
        }
    }
}