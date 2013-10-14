//-----------------------------------------------------------------------------
// <copyright file="MessageQueueTargetBlockTests.cs" 
//            company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq.UnitTests
{
    using System;
    using System.Messaging;
    using System.Threading.Tasks.Dataflow;

    using log4net.Config;

    using Moq;
    using Moq.Protected;

    using Xunit;

    public class MessageQueueTargetBlockTests
    {
        private const string QueueName = @".\private$\test";

        private readonly MessageQueueTargetBlock<string> block;

        private readonly Mock<MessageQueueFactory> mockMessageQueueFactory; 

        public MessageQueueTargetBlockTests()
        {
            BasicConfigurator.Configure();
            this.mockMessageQueueFactory = new Mock<MessageQueueFactory>(MockBehavior.Strict);
            this.block = new MessageQueueTargetBlock<string>(this.mockMessageQueueFactory.Object, QueueName);
        }

        [Fact]
        public void BlockSendsMessageToMessageQueue()
        {
            var mockMessageQueue = new Mock<MessageQueueBase>(MockBehavior.Strict);
            this.mockMessageQueueFactory.Setup(f => f.CreateMessageQueue(QueueName, QueueAccessMode.Send))
                .Returns(mockMessageQueue.Object);
            mockMessageQueue.Protected().Setup("Dispose", true);
            mockMessageQueue.Setup(q => q.Send("Hello!")).Verifiable();
            this.block.Post("Hello!");
            this.block.Complete();
            Assert.True(this.block.Completion.Wait(TimeSpan.FromSeconds(30.0)));
            mockMessageQueue.Verify(q => q.Send("Hello!"), Times.Once());
        }
    }
}
