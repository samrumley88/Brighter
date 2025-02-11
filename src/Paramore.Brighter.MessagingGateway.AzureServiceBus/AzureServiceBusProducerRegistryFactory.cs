﻿using System.Collections.Generic;
using Paramore.Brighter.MessagingGateway.AzureServiceBus.ClientProvider;

namespace Paramore.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusProducerRegistryFactory : IAmAProducerRegistryFactory
    {
        private readonly IServiceBusClientProvider _clientProvider;
        private readonly IEnumerable<AzureServiceBusPublication> _asbPublications;

        /// <summary>
        /// Creates a producer registry initialized with producers for ASB derived from the publications
        /// </summary>
        /// <param name="configuration">The configuration of the connection to AWS</param>
        /// <param name="asbPublications">A set of publications - topics on the server - to configure</param>
        public AzureServiceBusProducerRegistryFactory(
            AzureServiceBusConfiguration configuration, 
            IEnumerable<AzureServiceBusPublication> asbPublications)
        {
             _clientProvider = new ServiceBusConnectionStringClientProvider(configuration.ConnectionString);
             _asbPublications = asbPublications;
        }

        /// <summary>
        /// Creates a producer registry initialized with producers for ASB derived from the publications
        /// </summary>
        /// <param name="clientProvider">The connection to AWS</param>
        /// <param name="asbPublications">A set of publications - topics on the server - to configure</param>
        public AzureServiceBusProducerRegistryFactory(
            IServiceBusClientProvider clientProvider,
            IEnumerable<AzureServiceBusPublication> asbPublications)
        {
            _clientProvider = clientProvider;
            _asbPublications = asbPublications;
        }
        
        
        /// <summary>
        /// Creates message producers.
        /// </summary>
        /// <returns>A has of middleware clients by topic, for sending messages to the middleware</returns>
        public IAmAProducerRegistry Create()
        {
            var producers = new Dictionary<string, IAmAMessageProducer>();
            foreach (var publication in _asbPublications)
            {
                producers[publication.Topic] = AzureServiceBusMessageProducerFactory.Get(_clientProvider, publication);;
            }

            return new ProducerRegistry(producers);
        }
    }
}
