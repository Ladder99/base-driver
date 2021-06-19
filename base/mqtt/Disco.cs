using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.@base.mqtt
{
    public class Disco
    {
        private class MQTTDiscoItem
        {
            public long added;
            public long seen;
            public string machineId;
            public string arrivalTopic;
            public string changeTopic;
        }
        
        private Dictionary<string, MQTTDiscoItem> _items = new Dictionary<string, MQTTDiscoItem>();

        private Broker _broker;
        private string _base_topic;
        
        public Disco(Broker broker, string base_topic = "disco")
        {
            _broker = broker;
            _base_topic = base_topic;
        }

        public async Task AddAsync(string machineId)
        {
            if (!_items.ContainsKey(machineId))
            {
                var epoch = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

                _items.Add(machineId, new MQTTDiscoItem()
                {
                    machineId = machineId,
                    added = epoch,
                    seen = epoch,
                    arrivalTopic = $"{_base_topic}/{machineId}-all",
                    changeTopic = $"{_base_topic}/{machineId}"
                });
            }
            else
            {
                _items[machineId].seen = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            }

            //TODO: object key and machineId are redundant
            
            string topic = $"{_base_topic}/DISCO";
            string payload = JObject.FromObject(_items).ToString();
            await _broker.PublishAsync(topic, payload);
        }
    }
}