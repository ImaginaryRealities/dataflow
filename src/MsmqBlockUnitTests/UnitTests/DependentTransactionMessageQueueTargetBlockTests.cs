//-----------------------------------------------------------------------------
// <copyright file="DependentTransactionMessageQueueTargetBlockTests.cs"
//            company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.Msmq.UnitTests
{
    using System;
    using System.Messaging;
    using System.Threading.Tasks.Dataflow;

    using Moq;
    using Moq.Protected;

    using Xunit;

    public class DependentTransactionMessageQueueTargetBlockTests
    {
        private const string QueueName = @".\private$\test";

        private readonly DependentTransactionMessageQueueTargetBlock<string> block;

        private readonly Mock<MessageQueueFactory> mockMessageQueueFactory =
            new Mock<MessageQueueFactory>(MockBehavior.Strict);
 
        private readonly Mock<TransactionService> mockTransactionService =
            new Mock<TransactionService>(MockBehavior.Strict);

        public DependentTransactionMessageQueueTargetBlockTests()
        {
            this.block = new DependentTransactionMessageQueueTargetBlock<string>(
                this.mockTransactionService.Object,
                this.mockMessageQueueFactory.Object,
                QueueName);
        }

        [Fact]
        public void BlockSendsMessagesToTheMessageQueueAndCommitsTransaction()
        {
            var mockDependentTransaction = new Mock<DependentTransactionBase>();
            mockDependentTransaction.Setup(dt => dt.Complete()).Verifiable();
            var mockTransactionScope = new Mock<TransactionScopeBase>(MockBehavior.Strict);
            mockTransactionScope.Protected().Setup("Dispose", true);
            mockTransactionScope.Setup(ts => ts.Complete()).Verifiable();
            this.mockTransactionService.Setup(ts => ts.CreateTransactionScope(mockDependentTransaction.Object))
                .Returns(mockTransactionScope.Object);
            var mockMessageQueue = new Mock<MessageQueueBase>(MockBehavior.Strict);
            mockMessageQueue.Protected().Setup("Dispose", true);
            mockMessageQueue.Setup(q => q.Send("Hello!", MessageQueueTransactionType.Automatic)).Verifiable();
            this.mockMessageQueueFactory.Setup(f => f.CreateMessageQueue(QueueName, QueueAccessMode.Send))
                .Returns(mockMessageQueue.Object);
            Assert.True(this.block.Post(Tuple.Create("Hello!", mockDependentTransaction.Object)));
            this.block.Complete();
            Assert.True(this.block.Completion.Wait(TimeSpan.FromSeconds(30.0)));
            Assert.False(this.block.Completion.IsFaulted);
            mockMessageQueue.Verify(q => q.Send("Hello!", MessageQueueTransactionType.Automatic), Times.Once());
            mockTransactionScope.Verify(ts => ts.Complete(), Times.Once());
            mockDependentTransaction.Verify(dt => dt.Complete(), Times.Once());
        }

        [Fact]
        public void BlockRollsBackTransactionIfExceptionOccurs()
        {
            var mockDependentTransaction = new Mock<DependentTransactionBase>(MockBehavior.Strict);
            mockDependentTransaction.Setup(dt => dt.Rollback(It.IsAny<Exception>())).Verifiable();
            var mockTransactionScope = new Mock<TransactionScopeBase>(MockBehavior.Strict);
            mockTransactionScope.Protected().Setup("Dispose", true);
            mockTransactionScope.Setup(ts => ts.Complete()).Verifiable();
            this.mockTransactionService.Setup(ts => ts.CreateTransactionScope(mockDependentTransaction.Object))
                .Returns(mockTransactionScope.Object);
            var mockMessageQueue = new Mock<MessageQueueBase>(MockBehavior.Strict);
            mockMessageQueue.Protected().Setup("Dispose", true);
            mockMessageQueue.Setup(q => q.Send("Hello!", MessageQueueTransactionType.Automatic))
                .Throws<InvalidOperationException>();
            this.mockMessageQueueFactory.Setup(f => f.CreateMessageQueue(QueueName, QueueAccessMode.Send))
                .Returns(mockMessageQueue.Object);
            Assert.True(this.block.Post(Tuple.Create("Hello!", mockDependentTransaction.Object)));
            this.block.Complete();
            Assert.Throws<AggregateException>(() => this.block.Completion.Wait(TimeSpan.FromSeconds(30.0)));
            Assert.True(this.block.Completion.IsFaulted);
            mockTransactionScope.Verify(ts => ts.Complete(), Times.Never());
            mockDependentTransaction.Verify(dt => dt.Rollback(It.IsAny<Exception>()), Times.Once());
        }
    }
}
