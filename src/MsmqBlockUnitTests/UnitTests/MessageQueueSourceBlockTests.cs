//-----------------------------------------------------------------------------
// <copyright file="MessageQueueSourceBlockTests.cs" 
//            company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq.UnitTests
{
    using System;
    using System.Globalization;
    using System.Messaging;
    using System.Threading.Tasks.Dataflow;

    using log4net.Config;

    using Moq;
    using Moq.Protected;

    using Xunit;

    public class MessageQueueSourceBlockTests
    {
        private const string QueueName = @".\private$\test";

        private readonly MessageQueueSourceBlock<string> block;

        private readonly Mock<MessageQueueBase> mockMessageQueue = new Mock<MessageQueueBase>(MockBehavior.Strict);

        private readonly Mock<MessageQueueFactory> mockMessageQueueFactory =
            new Mock<MessageQueueFactory>(MockBehavior.Strict);

        public MessageQueueSourceBlockTests()
        {
            BasicConfigurator.Configure();
            this.mockMessageQueueFactory.Setup(f => f.CreateMessageQueue(QueueName, QueueAccessMode.Receive))
                .Returns(this.mockMessageQueue.Object);
            this.mockMessageQueue.Protected().Setup("Dispose", true);
            this.block = new MessageQueueSourceBlock<string>(this.mockMessageQueueFactory.Object, QueueName);
        }

        [Fact]
        public void BlockOutputsMessageReceivedFromMsmqQueue()
        {
            var mockMessageEnumerator = new Mock<MessageEnumeratorBase>(MockBehavior.Strict);
            mockMessageEnumerator.Protected().Setup("Dispose", true);
            this.mockMessageQueue.Setup(x => x.GetMessageEnumerator()).Returns(mockMessageEnumerator.Object);
            var calls = 0;
            mockMessageEnumerator.Setup(x => x.MoveNext(It.IsAny<TimeSpan>()))
                .Returns(() => 0 == calls)
                .Callback(() => ++calls);
            const string TestMessage = "Test";
            this.mockMessageQueue.Setup(x => x.Receive(It.IsAny<TimeSpan>())).Returns(new Message(TestMessage));
            this.block.Start();
            var message = this.block.Receive(TimeSpan.FromSeconds(30.0));
            Assert.Equal(TestMessage, message);
            if (!this.block.Completion.IsCompleted)
            {
                this.block.Complete();
                Assert.True(this.block.Completion.Wait(TimeSpan.FromSeconds(30.0)));
            }

            Assert.False(this.block.Completion.IsFaulted);
        }

        [Fact]
        public void ExceptionCausesFaultToBlock()
        {
            var mockMessageEnumerator = new Mock<MessageEnumeratorBase>(MockBehavior.Strict);
            mockMessageEnumerator.Protected().Setup("Dispose", true);
            this.mockMessageQueue.Setup(x => x.GetMessageEnumerator()).Returns(mockMessageEnumerator.Object);
            mockMessageEnumerator.Setup(x => x.MoveNext(It.IsAny<TimeSpan>())).Returns(true);
            this.mockMessageQueue.Setup(x => x.Receive(It.IsAny<TimeSpan>())).Throws<InvalidOperationException>();
            this.block.Start();
            Assert.Throws<AggregateException>(() => this.block.Completion.Wait(TimeSpan.FromSeconds(30.0)));
            Assert.True(this.block.Completion.IsFaulted);
        }

        [Fact]
        public void ToStringReturnsQueueName()
        {
            Assert.Equal(QueueName, this.block.ToString());
        }
    }
}
