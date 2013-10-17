//-----------------------------------------------------------------------------
// <copyright file="TransactionalMessageQueueTargetBlockTests.cs"
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

    public class TransactionalMessageQueueTargetBlockTests
    {
        private readonly TransactionalMessageQueueTargetBlock<string> block;

        private readonly Mock<MessageQueueFactory> mockMessageQueueFactory =
            new Mock<MessageQueueFactory>(MockBehavior.Strict);

        private readonly Mock<TransactionService> mocktransactionService =
            new Mock<TransactionService>(MockBehavior.Strict); 

        public TransactionalMessageQueueTargetBlockTests()
        {
            this.block = new TransactionalMessageQueueTargetBlock<string>(
                this.mocktransactionService.Object,
                this.mockMessageQueueFactory.Object,
                @".\private$\test");
        }

        [Fact]
        public void BlockSendsMessageToTransactionalQueue()
        {
            var mockTransactionScope = new Mock<TransactionScopeBase>(MockBehavior.Strict);
            mockTransactionScope.Protected().Setup("Dispose", true);
            this.mocktransactionService.Setup(ts => ts.CreateTransactionScope()).Returns(mockTransactionScope.Object);
            mockTransactionScope.Setup(ts => ts.Complete()).Verifiable();
            var mockMessageQueue = new Mock<MessageQueueBase>(MockBehavior.Strict);
            mockMessageQueue.Protected().Setup("Dispose", true);
            this.mockMessageQueueFactory.Setup(f => f.CreateMessageQueue(@".\private$\test", QueueAccessMode.Send))
                .Returns(mockMessageQueue.Object);
            mockMessageQueue.Setup(q => q.Send("Hello!", MessageQueueTransactionType.Automatic)).Verifiable();
            Assert.True(this.block.Post("Hello!"));
            this.block.Complete();
            Assert.True(this.block.Completion.Wait(TimeSpan.FromSeconds(30.0)));
            mockTransactionScope.Verify(ts => ts.Complete(), Times.Once());
            mockMessageQueue.Verify(q => q.Send("Hello!", MessageQueueTransactionType.Automatic), Times.Once());
        }
    }
}
