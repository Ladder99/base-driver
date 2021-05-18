using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;

namespace l99.driver.@base
{
    public class Machines
    {
        private List<Machine> _machines;
        
        private Dictionary<string, dynamic> _propertyBag;

        private int _collectionInterval;

        public Machines(int collectionInterval = 1000)
        {
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
        
        public Machine Add(dynamic cfg)
        {
            //var machine = new Machine(this, cfg.enabled, cfg.id, cfg);
            var machine = (Machine) Activator.CreateInstance(Type.GetType(cfg.type), new object[] { this, cfg.enabled, cfg.id, cfg });
            _machines.Add(machine);
            return machine;
        }

        public void Run()
        {
            foreach (var machine in _machines.Where(x => x.Enabled))
            {
                machine.InitCollector();
            }

            while (true)
            {
                Thread.Sleep(_collectionInterval);

                foreach (var machine in _machines.Where(x => x.Enabled))
                {
                    machine.RunCollector();
                    machine.Handler.OnCollectorSweepCompleteInternal();
                }
            }
        }
        
        public async Task RunAsync()
        {
            List<Task> tasks = new List<Task>();

            foreach (var machine in _machines.Where(x => x.Enabled))
            {
                tasks.Add(runMachineAsync(machine));
            }

            await Task.WhenAll(tasks);
        }

        async Task runMachineAsync(Machine machine)
        {
            await machine.InitCollectorAsync();

            while (true)
            {
                await Task.Delay(machine.SweepMs).ConfigureAwait(false);
                await machine.RunCollectorAsync();
                await machine.Handler.OnCollectorSweepCompleteInternalAsync();
            }
        }
    }
}