using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using l99.driver.@base.mqtt;
using MoreLinq;
using Newtonsoft.Json.Linq;
using NLog;

namespace l99.driver.@base
{
    public class Machines
    {
        private ILogger _logger;
        private List<Machine> _machines;
        private Dictionary<string, dynamic> _propertyBag;
        private int _collectionInterval;
        private bool _isRunning = true;
        
        public Machines(int collectionInterval = 1000)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _collectionInterval = collectionInterval;
            _machines = new List<Machine>();
            _propertyBag = new Dictionary<string, dynamic>();
        }
        
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
        
        public Machine Add(dynamic cfg, Broker broker)
        {
            _logger.Debug($"Adding machine:\n{JObject.FromObject(cfg).ToString()}");
            var machine = (Machine) Activator.CreateInstance(Type.GetType(cfg.type), new object[] { this, cfg.enabled, cfg.id, cfg });
            machine["broker"] = broker;
            _machines.Add(machine);
            return machine;
        }

        public async Task RunAsync()
        {
            List<Task> tasks = new List<Task>();

            foreach (var machine in _machines.Where(x => x.Enabled))
            {
                tasks.Add(runMachineAsync(machine));
            }
            
            _logger.Info("Machine tasks running...");
            await Task.WhenAll(tasks);
        }

        async Task runMachineAsync(Machine machine)
        {
            await machine.InitCollectorAsync();

            while (_isRunning && machine.IsRunning)
            {
                await machine.RunCollectorAsync();
            }
        }

        void ShutdownAll()
        {
            _logger.Info("All machine tasks stopping...");
            _isRunning = false;
        }

        void Shutdown(string machineId)
        {
            _logger.Info($"Machine '{machineId}' tasks stopping...");
            _machines.FirstOrDefault(m => m.Id == machineId).Shutdown();
        }
    }
}