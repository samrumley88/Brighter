#region Licence
/* The MIT License (MIT)
Copyright � 2014 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the �Software�), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED �AS IS�, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using paramore.brighter.commandprocessor.tests.nunit.CommandProcessors.TestDoubles;
using paramore.brighter.commandprocessor.tests.nunit.MessageDispatch.TestDoubles;
using paramore.brighter.serviceactivator;
using paramore.brighter.serviceactivator.TestHelpers;

namespace paramore.brighter.commandprocessor.tests.nunit.MessageDispatch
{
    [TestFixture]
    public class DispatcherRestartConnectionTests
    {
        private Dispatcher _dispatcher;
        private FakeChannel _channel;
        private IAmACommandProcessor _commandProcessor;
        private Connection _connection;
        private Connection _newConnection;

        [SetUp]
        public void Establish()
        {
            _channel = new FakeChannel();
            _commandProcessor = new SpyCommandProcessor();

            var messageMapperRegistry = new MessageMapperRegistry(new SimpleMessageMapperFactory(() => new MyEventMessageMapper()));
            messageMapperRegistry.Register<MyEvent, MyEventMessageMapper>();

            _connection = new Connection(name: new ConnectionName("test"), dataType: typeof(MyEvent), noOfPerformers: 1, timeoutInMilliseconds: 1000, channelFactory: new InMemoryChannelFactory(_channel), channelName: new ChannelName("fakeChannel"), routingKey: "fakekey");
            _newConnection = new Connection(name: new ConnectionName("newTest"), dataType: typeof(MyEvent), noOfPerformers: 1, timeoutInMilliseconds: 1000, channelFactory: new InMemoryChannelFactory(_channel), channelName: new ChannelName("fakeChannel"), routingKey: "fakekey");
            _dispatcher = new Dispatcher(_commandProcessor, messageMapperRegistry, new List<Connection> { _connection, _newConnection });

            var @event = new MyEvent();
            var message = new MyEventMessageMapper().MapToMessage(@event);
            _channel.Add(message);

            _dispatcher.State.ShouldEqual(DispatcherState.DS_AWAITING);
            _dispatcher.Receive();
            Task.Delay(1000).Wait();
            _dispatcher.Shut("test");
            _dispatcher.Shut("newTest");
            Task.Delay(3000).Wait();
            _dispatcher.Consumers.Count().ShouldEqual(0); //sanity check
        }


        [Test]
        public void When_A_Message_Dispatcher_Restarts_A_Connection_After_All_Connections_Have_Stopped()
        {
            _dispatcher.Open("newTest");
            var @event = new MyEvent();
            var message = new MyEventMessageMapper().MapToMessage(@event);
            _channel.Add(message);
            Task.Delay(1000).Wait();


            //_should_have_consumed_the_messages_in_the_event_channel
            _channel.Length.ShouldEqual(0);
            //_should_have_a_running_state
            _dispatcher.State.ShouldEqual(DispatcherState.DS_RUNNING);
            //_should_have_only_one_consumer
           _dispatcher.Consumers.Count().ShouldEqual(1);
            //_should_have_two_connections
           _dispatcher.Connections.Count().ShouldEqual(2);
        }

        [TearDown]
        public void Cleanup()
        {
            if (_dispatcher?.State == DispatcherState.DS_RUNNING)
                _dispatcher.End().Wait();
        }
    }
}