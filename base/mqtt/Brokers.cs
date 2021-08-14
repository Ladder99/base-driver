using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;

namespace l99.driver.@base.mqtt
{
    public class Brokers
    {
        private ILogger _logger;
        private Dictionary<string, Broker> _brokers;
        private Dictionary<string, Dictionary<string, Broker>> _brokerGroups;
            
        public Brokers()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _brokers = new Dictionary<string, Broker>();
            _brokerGroups = new Dictionary<string, Dictionary<string, Broker>>();
        }

        public async Task<Broker> AddAsync(dynamic cfg)
        {
            dynamic cfg_machine = cfg.machine;
            dynamic cfg_broker = cfg.broker;
            
            var groupKey = $"{cfg_broker.ip}:{cfg_broker.port}";
            var key = $"{groupKey}/{cfg_machine.id}";

            //DONE: does not handle multiple brokers with different configurations correctly
            // e.g. if first broker is enabled=false, and second enabled=true, then broker will remain disabled
            if (_brokers.ContainsKey(key))
            {
                return _brokers[key];
            }
            else
            {
                _logger.Debug($"Adding broker for machine '{cfg_machine.id}':\n{JObject.FromObject(cfg_broker).ToString()}");
                
                Broker broker = new Broker(groupKey, key, cfg_broker);
                
                if(cfg_broker.auto_connect)
                    await broker.ConnectAsync();
                
                _brokers.Add(key, broker);

                if (!_brokerGroups.ContainsKey(groupKey))
                {
                    _brokerGroups.Add(groupKey, new Dictionary<string, Broker>());
                }

                if (!_brokerGroups[groupKey].ContainsKey(cfg_machine.id))
                {
                    _brokerGroups[groupKey].Add(cfg_machine.id, broker);
                }
                
                return broker;
            }
        }
    }
}