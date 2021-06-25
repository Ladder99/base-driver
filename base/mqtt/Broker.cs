using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Exceptions;
using NLog;

namespace l99.driver.@base.mqtt
{
    public class Broker
    {
        private ILogger _logger;
        private MqttClientOptions _options;
        private IMqttClient _client;

        public IMqttClient Client
        {
            get => _client;
        }

        private bool MQTT_CONNECT = false;
        private bool MQTT_PUBLISH_STATUS = false;
        private bool MQTT_PUBLISH_ARRIVALS = false;
        private bool MQTT_PUBLISH_CHANGES = false;
        private bool MQTT_PUBLISH_DISCO = false;

        public Broker(dynamic cfg)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _propertyBag = new Dictionary<string, dynamic>();
            _subscriptions = new Dictionary<string, List<Func<string, string, ushort, bool, Task>>>();
            
            MQTT_CONNECT = cfg.enabled;
            MQTT_PUBLISH_STATUS = cfg.pub_status;
            MQTT_PUBLISH_ARRIVALS = cfg.pub_arrivals;
            MQTT_PUBLISH_CHANGES = cfg.pub_changes;
            MQTT_PUBLISH_DISCO = cfg.pub_disco;

            this["disco"] = new Disco(this, cfg.disco_base_topic);

            var factory = new MqttFactory();
            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(cfg.ip, cfg.port)
                .Build();
            _client = factory.CreateMqttClient();
        }

        public async Task AddDiscoAsync(string machineId)
        {
            if (MQTT_PUBLISH_DISCO)
            {
                await this["disco"].AddAsync(machineId);
            }
        }

        public async Task ConnectAsync(string lwtTopic, string lwtPayload)
        {
            _options.WillMessage = new MqttApplicationMessageBuilder()
                .WithTopic(lwtTopic)
                .WithPayload(lwtPayload)
                .Build();

            await ConnectAsync();
        }

        public async Task ConnectAsync()
        {
            if (MQTT_CONNECT)
            {
                _logger.Debug($"Connecting broker: {_options.ChannelOptions}");
                try
                {
                    await _client.ConnectAsync(_options, CancellationToken.None);
                    _client.UseApplicationMessageReceivedHandler(async (e) =>
                    {
                        await handleIncomingMessage(e);
                    });
                }
                catch (MqttCommunicationException ex)
                {
                    _logger.Warn(ex, $"Broker connection failed: {_options.ChannelOptions}");
                }
            }
            else
            {
                _logger.Info($"Skipping broker connection: {_options.ChannelOptions}");
            }
        }

        private async Task handleIncomingMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            //TODO: handle wildcards
            if (_subscriptions.ContainsKey(e.ApplicationMessage.Topic))
            {
                foreach (var receiver in _subscriptions[e.ApplicationMessage.Topic])
                {
                    await receiver(e.ApplicationMessage.Topic,
                        e.ApplicationMessage.ConvertPayloadToString(),
                        (ushort)e.ApplicationMessage.QualityOfServiceLevel,
                        e.ApplicationMessage.Retain);
                }
            }
        }

        private Dictionary<string, List<Func<string, string, ushort, bool, Task>>> _subscriptions;
        
        public async Task SubscribeAsync(string topic, Func<string,string,ushort,bool,Task> receiver)
        {
            if (MQTT_CONNECT && _client.IsConnected)
            {
                //TODO: handle wildcards
                if (_subscriptions.ContainsKey(topic))
                {
                    _subscriptions[topic].Add(receiver);
                }
                else
                {
                    _subscriptions.Add(topic, new List<Func<string,string,ushort,bool,Task>>());
                    _subscriptions[topic].Add(receiver);
                }

                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            }
        }
        
        public async Task PublishAsync(string topic, string payload, bool retained = true)
        {
            _logger.Trace($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} PUB {payload.Length}b => {topic}\n{payload}");
            
            if (MQTT_CONNECT && _client.IsConnected)
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithRetainFlag(retained)
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .Build();
                
                await _client.PublishAsync(msg, CancellationToken.None);
            }
        }
        
        public async Task PublishArrivalStatusAsync(string topic, string payload, bool retained = true)
        {
            if (MQTT_CONNECT && MQTT_PUBLISH_STATUS && _client.IsConnected)
            {
                await PublishArrivalAsync(topic, payload, retained);
            }
        }

        public async Task PublishArrivalAsync(string topic, string payload, bool retained = true)
        {
            _logger.Trace($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} ARRIVE {payload.Length}b => {topic}\n{payload}");
            
            if (MQTT_CONNECT && MQTT_PUBLISH_ARRIVALS && _client.IsConnected)
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithRetainFlag(retained)
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .Build();
                
                await _client.PublishAsync(msg, CancellationToken.None);
            }
        }
        
        public async Task PublishChangeStatusAsync(string topic, string payload, bool retained = true)
        {
            if (MQTT_CONNECT && MQTT_PUBLISH_STATUS && _client.IsConnected)
            {
                await PublishChangeAsync(topic, payload, retained);
            }
        }
        
        public async Task PublishChangeAsync(string topic, string payload, bool retained = true)
        {
            _logger.Trace($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} CHANGE {payload.Length}b => {topic}\n{payload}");
            
            if (MQTT_CONNECT && MQTT_PUBLISH_CHANGES && _client.IsConnected)
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithRetainFlag(retained)
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .Build();
                
                await _client.PublishAsync(msg, CancellationToken.None);
            }
        }
        
        private Dictionary<string, dynamic> _propertyBag;
        
        public dynamic? this[string propertyBagKey]
        {
            get
            {
                if (_propertyBag.ContainsKey(propertyBagKey))
                {
                    return _propertyBag[propertyBagKey];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (_propertyBag.ContainsKey(propertyBagKey))
                {
                    _propertyBag[propertyBagKey] = value;
                }
                else
                {
                    _propertyBag.Add(propertyBagKey, value);
                }
            }
        }
    }
}