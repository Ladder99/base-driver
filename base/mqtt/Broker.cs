using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace l99.driver.@base.mqtt
{
    public class Broker
    {
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
                await _client.ConnectAsync(_options, CancellationToken.None);
            }
        }

        public async Task PublishAsync(string topic, string payload, bool retained = true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} PUB {payload.Length}b => {topic}");

            if (MQTT_CONNECT)
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
            if (MQTT_CONNECT && MQTT_PUBLISH_STATUS)
            {
                await PublishArrivalAsync(topic, payload, retained);
            }
        }

        public async Task PublishArrivalAsync(string topic, string payload, bool retained = true)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} ARRIVE {payload.Length}b => {topic}");

            if (MQTT_CONNECT && MQTT_PUBLISH_ARRIVALS)
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
            if (MQTT_CONNECT && MQTT_PUBLISH_STATUS)
            {
                await PublishChangeAsync(topic, payload, retained);
            }
        }
        
        public async Task PublishChangeAsync(string topic, string payload, bool retained = true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} CHANGE {payload.Length}b => {topic}");
            
            if (MQTT_CONNECT && MQTT_PUBLISH_CHANGES)
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