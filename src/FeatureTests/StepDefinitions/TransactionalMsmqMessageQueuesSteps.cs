//-----------------------------------------------------------------------------
// <copyright file="TransactionalMsmqMessageQueuesSteps.cs"
//            company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.FeatureTests.StepDefinitions
{
    using System;
    using System.Messaging;
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using System.Transactions;

    using ImaginaryRealities.Framework.Dataflow.Msmq;

    using NUnit.Framework;

    using TechTalk.SpecFlow;

    [Binding]
    public class TransactionalMsmqMessageQueuesSteps
    {
        private const string InputQueueName = @".\private$\transactional_input";

        private const string OutputQueueName = @".\private$\transactional_output";

        [Given(@"the input message queue exists")]
        public void GivenTheInputMessageQueueExists()
        {
            if (MessageQueue.Exists(InputQueueName))
            {
                return;
            }

            using (MessageQueue.Create(InputQueueName, true))
            {
            }
        }
        
        [Given(@"the input message queue is empty")]
        public void GivenTheInputMessageQueueIsEmpty()
        {
            using (var messageQueue = new MessageQueue(InputQueueName, QueueAccessMode.ReceiveAndAdmin))
            {
                messageQueue.Purge();
            }
        }
        
        [Given(@"the output message queue exists")]
        public void GivenTheOutputMessageQueueExists()
        {
            if (MessageQueue.Exists(OutputQueueName))
            {
                return;
            }

            using (MessageQueue.Create(OutputQueueName, true))
            {
            }
        }
        
        [Given(@"the output message queue is empty")]
        public void GivenTheOutputMessageQueueIsEmpty()
        {
            using (var messageQueue = new MessageQueue(OutputQueueName, QueueAccessMode.ReceiveAndAdmin))
            {
                messageQueue.Purge();
            }
        }
        
        [When(@"I publish a message to the input message queue")]
        public void WhenIPublishAMessageToTheInputMessageQueue()
        {
            using (var transactionScope = new TransactionScope())
            {
                using (var messageQueue = new MessageQueue(InputQueueName, QueueAccessMode.Send))
                {
                    using (var message = new Message("Hello!"))
                    {
                        messageQueue.Send(message, MessageQueueTransactionType.Automatic);
                    }
                }

                transactionScope.Complete();
            }

            var count = 0;
            using (var messageQueue = new MessageQueue(InputQueueName, QueueAccessMode.ReceiveAndAdmin))
            {
                using (var enumerator = messageQueue.GetMessageEnumerator2())
                {
                    while (enumerator.MoveNext())
                    {
                        ++count;
                    }
                }
            }

            Assert.That(count, Is.EqualTo(1));
        }
        
        [Then(@"a message is published to the output message queue")]
        public void ThenAMessageIsPublishedToTheOutputMessageQueue()
        {
            var sourceBlock = new DependentTransactionMessageQueueSourceBlock<string>(InputQueueName);
            var targetBlock = new DependentTransactionMessageQueueTargetBlock<string>(OutputQueueName);
            sourceBlock.LinkTo(targetBlock, new DataflowLinkOptions { PropagateCompletion = true });
            sourceBlock.Start();
            Thread.Sleep(TimeSpan.FromSeconds(10.0));
            sourceBlock.Complete();
            Assert.That(targetBlock.Completion.Wait(TimeSpan.FromSeconds(30.0)), Is.True);
            Assert.That(targetBlock.Completion.IsFaulted, Is.False);
            var count = 0;
            using (var messageQueue = new MessageQueue(OutputQueueName, QueueAccessMode.ReceiveAndAdmin))
            {
                using (var enumerator = messageQueue.GetMessageEnumerator2())
                {
                    while (enumerator.MoveNext())
                    {
                        ++count;
                    }
                }
            }

            Assert.That(count, Is.EqualTo(1));
        }
        
        [Then(@"the input message queue is empty")]
        public void ThenTheInputMessageQueueIsEmpty()
        {
            using (var messageQueue = new MessageQueue(InputQueueName, QueueAccessMode.ReceiveAndAdmin))
            {
                using (var enumerator = messageQueue.GetMessageEnumerator2())
                {
                    Assert.That(enumerator.MoveNext(), Is.False);
                }
            }
        }

        [When(@"an exception occurs in the pipeline")]
        public void WhenAnExceptionOccursInThePipeline()
        {
            var sourceBlock = new DependentTransactionMessageQueueSourceBlock<string>(InputQueueName);
            var actionBlock = new ActionBlock<Tuple<string, DependentTransactionBase>>(
                t =>
                    {
                        try
                        {
                            using (var transactionScope = new TransactionScope(t.Item2))
                            {
                                throw new InvalidOperationException();
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            t.Item2.Rollback(ex);
                            throw;
                        }
                    });
            sourceBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });
            sourceBlock.Start();
            Thread.Sleep(TimeSpan.FromSeconds(10.0));
            sourceBlock.Complete();
            var exception =
                Assert.Throws<AggregateException>(() => actionBlock.Completion.Wait(TimeSpan.FromSeconds(30.0)));
            Assert.That(actionBlock.Completion.IsFaulted, Is.True);
        }

        [Then(@"the input message is remains in the input queue")]
        public void ThenTheInputMessageIsRemainsInTheInputQueue()
        {
            var count = 0;
            using (var messageQueue = new MessageQueue(InputQueueName, QueueAccessMode.ReceiveAndAdmin))
            {
                using (var enumerator = messageQueue.GetMessageEnumerator2())
                {
                    while (enumerator.MoveNext())
                    {
                        ++count;
                    }
                }
            }

            Assert.That(count, Is.EqualTo(1));
        }
    }
}
