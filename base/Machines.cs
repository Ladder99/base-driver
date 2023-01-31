// ReSharper disable once CheckNamespace

namespace l99.driver.@base;

public class Machines
{
    private readonly ILogger _logger;
    private readonly List<Machine> _machines;
    private readonly Dictionary<string, dynamic> _propertyBag;
    private bool _isRunning = true;

    private Machines()
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
                return _propertyBag[propertyBagKey];
            return null;
        }

        set
        {
            if (_propertyBag.ContainsKey(propertyBagKey))
            {
#pragma warning disable CS8601
                _propertyBag[propertyBagKey] = value;
#pragma warning restore CS8601
            }
            else
            {
                _propertyBag.Add(propertyBagKey, value);
            }
        }
    }

    private Machine? Add(dynamic cfg)
    {
        _logger.Debug($"Adding machine:\n{JObject.FromObject(cfg.machine).ToString()}");
        var machine = (Machine) Activator.CreateInstance(
            Type.GetType(cfg.machine.type),
            new object[] {this, cfg});

        if (machine != null)
            _machines.Add(machine);
        else
            _logger.Error($"Unable to create machine '{cfg.machine.type}'");

        return machine;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        foreach (var machine in _machines.Where(x => x.Enabled)) tasks.Add(RunMachineAsync(machine));

        _logger.Info("Machine tasks running...");

        await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(Timeout.Infinite, stoppingToken));
        _logger.Info("Machine tasks stopping...");
    }

    private async Task RunMachineAsync(Machine machine)
    {
        await machine.InitStrategyAsync();

        while (_isRunning && machine.IsRunning) await machine.RunStrategyAsync();
    }

    // ReSharper disable once UnusedMember.Local
    private void ShutdownAll()
    {
        _logger.Info("All machine tasks stopping...");
        _isRunning = false;
    }

    // ReSharper disable once UnusedMember.Local
    private void Shutdown(string machineId)
    {
        _logger.Info($"Machine '{machineId}' tasks stopping...");
        _machines.FirstOrDefault(m => m.Id == machineId)?.Shutdown();
    }

    public static async Task<Machines> CreateMachines(dynamic config)
    {
        var logger = LogManager.GetCurrentClassLogger();

        var assemblyName = typeof(Machines).Assembly.GetName().Name;
        var machineConfigs = new List<dynamic>();

        foreach (var machineConf in config["machines"])
        {
            // extract primary types as strings from each machine configuration
            var prebuiltConfig = new
            {
                machine = new
                {
                    enabled = machineConf.ContainsKey("enabled") ? machineConf["enabled"] : false,
                    type = machineConf.ContainsKey("type")
                        ? machineConf["type"]
                        : $"l99.driver.@base.Machine, {assemblyName}",
                    id = machineConf.ContainsKey("id") ? machineConf["id"] : Guid.NewGuid().ToString(),
                    strategy = machineConf.ContainsKey("strategy")
                        ? machineConf["strategy"]
                        : $"l99.driver.@base.Strategy, {assemblyName}",
                    handler = machineConf.ContainsKey("handler")
                        ? machineConf["handler"]
                        : $"l99.driver.@base.Handler, {assemblyName}",
                    transport = machineConf.ContainsKey("transport")
                        ? machineConf["transport"]
                        : $"l99.driver.@base.Transport, {assemblyName}"
                }
            };

            // iterate previously identified primary type strings and extract each section
            var builtConfig = new
            {
                prebuiltConfig.machine,
                type = machineConf.ContainsKey(prebuiltConfig.machine.type)
                    ? machineConf[prebuiltConfig.machine.type]
                    : null,
                strategy = machineConf.ContainsKey(prebuiltConfig.machine.strategy)
                    ? machineConf[prebuiltConfig.machine.strategy]
                    : null,
                collectors = new Dictionary<string, object>(),
                handler = machineConf.ContainsKey(prebuiltConfig.machine.handler)
                    ? machineConf[prebuiltConfig.machine.handler]
                    : null,
                transport = machineConf.ContainsKey(prebuiltConfig.machine.transport)
                    ? machineConf[prebuiltConfig.machine.transport]
                    : null
            };

            // iterate strategy collector string types and extract section for each collector
            if (builtConfig.strategy != null)
                foreach (var collectorType in builtConfig.strategy["collectors"])
                    if (machineConf.ContainsKey(collectorType))
                        builtConfig.collectors.Add(collectorType, machineConf[collectorType]);

            // ReSharper disable once RedundantToStringCall
            logger.Trace($"Machine configuration built:\n{JObject.FromObject(builtConfig).ToString()}");

            machineConfigs.Add(builtConfig);
        }

        var machines = new Machines();

        foreach (var cfg in machineConfigs)
        {
            logger.Trace($"Creating machine from config:\n{JObject.FromObject(cfg).ToString()}");

            Machine machine = machines.Add(cfg);

            if (machine != null)
            {
                try
                {
                    Type transportType = Type.GetType(cfg.machine.transport);
                    Type strategyType = Type.GetType(cfg.machine.strategy);
                    Type handlerType = Type.GetType(cfg.machine.handler);
                
                    await machine.AddTransportAsync(transportType);
                    await machine.AddStrategyAsync(strategyType);
                    await machine.AddHandlerAsync(handlerType);
                }
                catch (Exception e)
                {
                    logger.Error($"[{machine.Id}] Failed to create");
                    machine.Disable();
                }
            }
        }

        return machines;
    }
}