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
        private dynamic _options;
        private IMqttClient _client;

        public IMqttClient Client
        {
            get => _client;
        }

        private bool MQTT_CONNECT = false;
        private bool MQTT_PUBLISH_STATUS = false;
        private bool MQTT_PUBLISH_ARRIVALS = false;
        private bool MQTT_PUBLISH_CHANGES = false;

        public Broker(dynamic cfg)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _propertyBag = new Dictionary<string, dynamic>();
            
            MQTT_CONNECT = cfg.enabled;
            MQTT_PUBLISH_STATUS = cfg.pub_status;
            MQTT_PUBLISH_ARRIVALS = cfg.pub_arrivals;
            MQTT_PUBLISH_CHANGES = cfg.pub_changes;
            
            var factory = new MqttFactory();
            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(cfg.ip, cfg.port)
                .Build();
            _client = factory.CreateMqttClient();
        }

        public async Task ConnectAsync()
        {
            if (MQTT_CONNECT)
            {
                _logger.Debug($"Connecting broker: {_options.ChannelOptions}");
                try
                {
                    await _client.ConnectAsync(_options, CancellationToken.None);
                }
                catch (MqttCommunicationException ex)
                {
                    _logger.Warn(ex,$"Broker connection failed: {_options.ChannelOptions}");
                }
            }
            else
            {
                _logger.Info($"Skipping broker connection: {_options.ChannelOptions}");
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