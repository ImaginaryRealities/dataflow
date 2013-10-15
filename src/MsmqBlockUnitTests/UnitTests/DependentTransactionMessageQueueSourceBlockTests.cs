//-----------------------------------------------------------------------------
// <copyright file="DependentTransactionMessageQueueSourceBlockTests.cs"
//            company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq.UnitTests
{
    using System;
    using System.Messaging;
    using System.Threading.Tasks.Dataflow;
    using System.Transactions;

    using log4net.Config;

    using Moq;
    using Moq.Protected;

    using Xunit;

    public class DependentTransactionMessageQueueSourceBlockTests
    {
        private const string QueueName = @".\private$\test";

        private readonly DependentTransactionMessageQueueSourceBlock<string> block;

        private readonly Mock<MessageQueueFactory> mockMessageQueueFactory =
            new Mock<MessageQueueFactory>(MockBehavior.Strict);

        private readonly Mock<TransactionService> mockTransactionService =
            new Mock<TransactionService>(MockBehavior.Strict);

        public DependentTransactionMessageQueueSourceBlockTests()
        {
            BasicConfigurator.Configure();
            this.block = new DependentTransactionMessageQueueSourceBlock<string>(
                this.mockMessageQueueFactory.Object,
                this.mockTransactionService.Object,
                QueueName);
        }

        [Fact]
        public void BlockOutputsMessageReceivedFromQueue()
        {
            var mockTransactionScope = new Mock<TransactionScopeBase>(MockBehavior.Strict);
            this.mockTransactionService.Setup(s => s.CreateTransactionScope()).Returns(mockTransactionScope.Object);
            mockTransactionScope.Protected().Setup("Dispose", true);
            mockTransactionScope.Setup(s => s.Complete()).Verifiable();
            var mockMessageQueue = new Mock<MessageQueueBase>(MockBehavior.Strict);
            this.mockMessageQueueFactory.Setup(f => f.CreateMessageQueue(QueueName, QueueAccessMode.Receive))
                .Returns(mockMessageQueue.Object);
            mockMessageQueue.Protected().Setup("Dispose", true);
            var message = new Message("Hello!");
            mockMessageQueue.Setup(q => q.Receive(It.IsAny<TimeSpan>(), MessageQueueTransactionType.Automatic))
                .Returns(message);
            var mockEnumerator = new Mock<MessageEnumeratorBase>(MockBehavior.Strict);
            mockMessageQueue.Setup(q => q.GetMessageEnumerator()).Returns(mockEnumerator.Object);
            var count = 0;
            mockEnumerator.Setup(e => e.MoveNext(It.IsAny<TimeSpan>()))
                .Returns(() => 0 == count)
                .Callback(() => ++count);
            mockEnumerator.Protected().Setup("Dispose", true);
            var mockDependentTransaction = new Mock<DependentTransactionBase>(MockBehavior.Strict);
            this.mockTransactionService.Setup(
                s => s.CreateDependentTransaction(DependentCloneOption.BlockCommitUntilComplete))
                .Returns(mockDependentTransaction.Object);
            this.block.Start();
            var data = this.block.Receive(TimeSpan.FromSeconds(30.0));
            Assert.Equal("Hello!", data.Item1);
            Assert.Same(mockDependentTransaction.Object, data.Item2);
            this.block.Complete();
            Assert.True(this.block.Completion.Wait(TimeSpan.FromSeconds(30.0)));
            mockTransactionScope.Verify(s => s.Complete(), Times.Once());
        }
    }
}
