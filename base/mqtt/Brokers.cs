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
            
        public Brokers()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _brokers = new Dictionary<string, Broker>();
        }

        public async Task<Broker> AddAsync(dynamic cfg)
        {
            var key = cfg.ip + ":" + cfg.port;

            //TODO: does not handle multiple brokers with different configurations correctly
            // e.g. if first broker is enabled=false, and second enabled=true, then broker will remain disabled
            if (_brokers.ContainsKey(key))
            {
                return _brokers[key];
            }
            else
            {
                _logger.Debug($"Adding broker:\n{JObject.FromObject(cfg).ToString()}");
                Broker broker = new Broker(cfg);
                if(cfg.auto_connect)
                    await broker.ConnectAsync();
                _brokers.Add(key, broker);
                return broker;
            }
        }
    }
}