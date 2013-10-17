Feature: MSMQ Message Queues

Scenario: Send messages to and receive messages from an MSMQ message queue
	Given the message queue exists
	And the message queue is empty
	When I publish messages to the message queue
		| Message              |
		| Hello, World!        |
		| My name is Michael    |
		| Goodbye cruel world! |
	Then I will receive the messages from the message queue
		| Message              |
		| Hello, World!        |
		| My name is Michael   |
		| Goodbye cruel world! |

Scenario: Execute a pipeline within a transaction
	Given the input message queue exists
	And the input message queue is empty
	And the output message queue exists
	And the output message queue is empty
	When I publish a message to the input message queue
	Then a message is published to the output message queue
	And the input message queue is empty

Scenario: Transaction rolls back if an error occurs
	Given the input message queue exists
	And the input message queue is empty
	When I publish a message to the input message queue
	And an exception occurs in the pipeline
	Then the input message is remains in the input queue