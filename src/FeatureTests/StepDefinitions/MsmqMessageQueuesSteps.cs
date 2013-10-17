//-----------------------------------------------------------------------------
// <copyright file="MsmqMessageQueuesSteps.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

namespace ImaginaryRealities.Framework.Dataflow.FeatureTests.StepDefinitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Messaging;
    using System.Threading.Tasks.Dataflow;

    using ImaginaryRealities.Framework.Dataflow.Msmq;

    using NUnit.Framework;

    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Assist;

    [Binding]
    public class MsmqMessageQueuesSteps
    {
        private const string QueueName = @".\private$\test";

        [Given(@"the message queue exists")]
        public void GivenTheMessageQueueExists()
        {
            if (MessageQueue.Exists(QueueName))
            {
                return;
            }

            using (MessageQueue.Create(QueueName))
            {
            }
        }

        [Given(@"the message queue is empty")]
        public void GivenTheMessageQueueIsEmpty()
        {
            using (var messageQueue = new MessageQueue(QueueName, QueueAccessMode.ReceiveAndAdmin))
            {
                messageQueue.Purge();
            }
        }
        
        [Then(@"I will receive the messages from the message queue")]
        public void ThenIWillReceiveTheMessagesFromTheMessageQueue(Table table)
        {
            var sourceBlock = new MessageQueueSourceBlock<string>(QueueName);
            sourceBlock.Start();
            var messages = new List<string>(table.RowCount);
            var timeout = TimeSpan.FromSeconds(10.0);
            for (var i = 0; i < table.RowCount; i++)
            {
                try
                {
                    messages.Add(sourceBlock.Receive(timeout));
                }
                catch (TimeoutException)
                {
                    break;
                }
            }

            sourceBlock.Complete();
            Assert.IsTrue(sourceBlock.Completion.Wait(TimeSpan.FromSeconds(30.0)));
            Assert.AreEqual(table.RowCount, messages.Count);
            Assert.That(messages, Is.EquivalentTo(table.Rows.Select(r => r["Message"])));
        }

        [When(@"I publish messages to the message queue")]
        public void WhenIPublishMessagesToTheMessageQueue(Table table)
        {
            var targetBlock = new MessageQueueTargetBlock<string>(QueueName);
            foreach (var row in table.Rows)
            {
                targetBlock.Post(row["Message"]);
            }

            targetBlock.Complete();
            Assert.True(targetBlock.Completion.Wait(TimeSpan.FromSeconds(30.0)));
        }
    }
}
