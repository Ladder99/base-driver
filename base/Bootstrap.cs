using System.IO;
using Microsoft.Extensions.Configuration;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// ReSharper disable once CheckNamespace
namespace l99.driver.@base;

// ReSharper disable once ClassNeverInstantiated.Global
public class Bootstrap
{
    private static ILogger _logger = null!;

    public static async Task Stop()
    {
        LogManager.Shutdown();
    }
    
    public static async Task<dynamic> Start(string[] args)
    {
        DetectArch();
        string nlogFile = GetArgument(args, "--nlog", "nlog.config");
        _logger = SetupLogger(nlogFile);
        string configFiles = GetArgument(args, "--config", "config.system.yml,config.user.yml,config.machines.yml");
        dynamic config = ReadConfig(configFiles.Split(','));
        return config;
    }

    private static void DetectArch()
    {
        Console.WriteLine($"Bitness: {(IntPtr.Size == 8 ? "64-bit" : "32-bit")}");
    }

    private static string GetArgument(string[] args, string optionName, string defaultValue)
    {
        var value = args.SkipWhile(i => i != optionName).Skip(1).Take(1).FirstOrDefault();
        var optionValue = string.IsNullOrEmpty(value) ? defaultValue : value;
        Console.WriteLine($"Argument '{optionName}' = '{optionValue}'");
        return optionValue;
    }

    private static Logger SetupLogger(string configFile)
    {
        LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configFile);

        var config = new ConfigurationBuilder().Build();

        return LogManager.Setup()
            .SetupExtensions(ext => ext.RegisterConfigSettings(config))
            .GetCurrentClassLogger();
    }

    private static dynamic ReadConfig(string[] configFiles)
    {
        var yaml = "";
        foreach (var configFile in configFiles)
        {
            yaml += File.ReadAllText(configFile);
        }

        var stringReader = new StringReader(yaml);
        var parser = new Parser(stringReader);
        var mergingParser = new MergingParser(parser);
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var config = deserializer.Deserialize(mergingParser);
        
        _logger.Trace($"Deserialized configuration:\n{JObject.FromObject(config ?? throw new InvalidOperationException("Configuration cannot be null."))}");
        return config;
    }
}