using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;

namespace l99.driver.@base
{
    public class Machines
    {
        private ILogger _logger;
        private List<Machine> _machines;
        private Dictionary<string, dynamic> _propertyBag;
        private bool _isRunning = true;
        
        public Machines()
        {
            _logger = LogManager.GetCurrentClassLogger();
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
        
        public Machine Add(dynamic cfg)
        {
            _logger.Debug($"Adding machine:\n{JObject.FromObject(cfg.machine).ToString()}");
            var machine = (Machine) Activator.CreateInstance(Type.GetType(cfg.machine.type), new object[] { this, cfg.machine.enabled, cfg.machine.id, cfg });
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
            await machine.InitStrategyAsync();

            while (_isRunning && machine.IsRunning)
            {
                await machine.RunStrategyAsync();
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
        
        public static async Task<Machines> CreateMachines(dynamic config)
        {
            var logger = LogManager.GetCurrentClassLogger();
            
            var assembly_name = typeof(Machines).Assembly.GetName().Name;
            var machine_confs = new List<dynamic>();

            foreach (dynamic machine_conf in config["machines"])
            {
                var prebuilt_config = new
                {
                    machine = new {
                        enabled = machine_conf.ContainsKey("enabled") ? machine_conf["enabled"] : false,
                        type = machine_conf.ContainsKey("type") ? machine_conf["type"] : $"l99.driver.@base.Machine, {assembly_name}",
                        id = machine_conf.ContainsKey("id") ? machine_conf["id"] : Guid.NewGuid().ToString(),
                        strategy = machine_conf.ContainsKey("strategy") ? machine_conf["strategy"] : $"l99.driver.@base.Strategy, {assembly_name}",
                        handler = machine_conf.ContainsKey("handler") ? machine_conf["handler"] : $"l99.driver.@base.Handler, {assembly_name}",
                        transport = machine_conf.ContainsKey("transport") ? machine_conf["transport"] : $"l99.driver.@base.Transport, {assembly_name}"
                    }
                };

                var built_config = new
                {
                    prebuilt_config.machine,
                    type = machine_conf.ContainsKey(prebuilt_config.machine.type)
                        ? machine_conf[prebuilt_config.machine.type]
                        : null,
                    strategy = machine_conf.ContainsKey(prebuilt_config.machine.strategy)
                        ? machine_conf[prebuilt_config.machine.strategy]
                        : null,
                    handler = machine_conf.ContainsKey(prebuilt_config.machine.handler)
                        ? machine_conf[prebuilt_config.machine.handler]
                        : null,
                    transport = machine_conf.ContainsKey(prebuilt_config.machine.transport)
                        ? machine_conf[prebuilt_config.machine.transport]
                        : null
                };

                logger.Trace($"Machine configuration built:\n{JObject.FromObject(built_config).ToString()}");
                
                machine_confs.Add(built_config);
            }

            Machines machines = new Machines();
            
            foreach (var cfg in machine_confs)
            {
                logger.Trace($"Creating machine from config:\n{JObject.FromObject(cfg).ToString()}");
                
                Machine machine = machines.Add(cfg);
                await machine.AddTransportAsync(Type.GetType(cfg.machine.transport), cfg);
                await machine.AddStrategyAsync(Type.GetType(cfg.machine.strategy), cfg);
                await machine.AddHandlerAsync(Type.GetType(cfg.machine.handler), cfg);
            }

            return machines;
        }
    }
}