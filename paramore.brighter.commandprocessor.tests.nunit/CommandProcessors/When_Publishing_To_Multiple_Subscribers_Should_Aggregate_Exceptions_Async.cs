﻿#region Licence
/* The MIT License (MIT)
Copyright © 2015 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System;
using Nito.AsyncEx;
using NUnit.Framework;
using paramore.brighter.commandprocessor.tests.nunit.CommandProcessors.TestDoubles;
using TinyIoC;

namespace paramore.brighter.commandprocessor.tests.nunit.CommandProcessors
{
    [TestFixture]
    public class PublishingToMultipleSubscribersAsyncTests
    {
        private CommandProcessor _commandProcessor;
        private readonly MyEvent _myEvent = new MyEvent();
        private Exception _exception;

        [SetUp]
        public void Establish()
        {
            var registry = new SubscriberRegistry();
            registry.RegisterAsync<MyEvent, MyEventHandlerAsync>();
            registry.RegisterAsync<MyEvent, MyOtherEventHandlerAsync>();
            registry.RegisterAsync<MyEvent, MyThrowingEventHandlerAsync>();

            var container = new TinyIoCContainer();
            var handlerFactory = new TinyIocHandlerFactoryAsync(container);
            container.Register<IHandleRequestsAsync<MyEvent>, MyEventHandlerAsync>("MyEventHandler");
            container.Register<IHandleRequestsAsync<MyEvent>, MyOtherEventHandlerAsync>("MyOtherHandler");
            container.Register<IHandleRequestsAsync<MyEvent>, MyThrowingEventHandlerAsync>("MyThrowingHandler");

            _commandProcessor = new CommandProcessor(registry, handlerFactory, new InMemoryRequestContextFactory(), new PolicyRegistry());
        }

        [Test]
        public void When_Publishing_To_Multiple_Subscribers_Should_Aggregate_Exceptions_Async()
        {
            _exception = Catch.Exception(() => AsyncContext.Run(async () => await _commandProcessor.PublishAsync(_myEvent)));

            //_should_throw_an_aggregate_exception
            _exception.ShouldBeOfExactType(typeof(AggregateException));
            //_should_have_an_inner_exception_from_the_handler
            ((AggregateException)_exception).InnerException.ShouldBeOfExactType(typeof(InvalidOperationException));
            //_should_publish_the_command_to_the_first_event_handler
            MyEventHandlerAsync.ShouldReceive(_myEvent).ShouldBeTrue();
            //_should_publish_the_command_to_the_second_event_handler
            MyOtherEventHandlerAsync.ShouldReceive(_myEvent).ShouldBeTrue();
        }
    }
}