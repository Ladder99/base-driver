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
        private string _baseTopic;
        
        public Disco(Broker broker, string baseTopic = "disco")
        {
            _broker = broker;
            _baseTopic = baseTopic;
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
                    arrivalTopic = $"{_baseTopic}/{machineId}-all",
                    changeTopic = $"{_baseTopic}/{machineId}"
                });
            }
            else
            {
                _items[machineId].seen = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            }

            //TODO: object key and machineId are redundant
            
            string topic = $"{_baseTopic}/$disco";
            string payload = JObject.FromObject(_items).ToString();
            await _broker.PublishAsync(topic, payload);
        }
    }
}