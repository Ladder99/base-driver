using System.IO;
using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace l99.driver.@base;

public class Bootstrap
{
    private static ILogger _logger;

    public static async Task Stop()
    {
        LogManager.Shutdown();
    }
    
    public static async Task<dynamic> Start(string[] args)
    {
        detectArch();
        string nlog_file = getArgument(args, "--nlog", "nlog.config");
        _logger = setupLogger(nlog_file);
        string config_files = getArgument(args, "--config", "config.system.yml,config.user.yml,config.machines.yml");
        dynamic config = readConfig(config_files.Split(','));
        return config;
    }
    
    static void detectArch()
    {
        Console.WriteLine($"Bitness: {(IntPtr.Size == 8 ? "64-bit" : "32-bit")}");
    }
    
    static string getArgument(string[] args, string option_name, string defaultValue)
    {
        var value = args.SkipWhile(i => i != option_name).Skip(1).Take(1).FirstOrDefault();
        var option_value = string.IsNullOrEmpty(value) ? defaultValue : value;
        Console.WriteLine($"Argument '{option_name}' = '{option_value}'");
        return option_value;
    }
    
    static Logger setupLogger(string config_file)
    {
        LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(config_file);

        var config = new ConfigurationBuilder().Build();

        return LogManager.Setup()
            .SetupExtensions(ext => ext.RegisterConfigSettings(config))
            .GetCurrentClassLogger();
    }

    static dynamic readConfig(string[] config_files)
    {
        var yaml = "";
        foreach (var config_file in config_files)
        {
            yaml += File.ReadAllText(config_file);
        }

        var string_reader = new StringReader(yaml);
        var parser = new Parser(string_reader);
        var merging_parser = new MergingParser(parser);
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var config = deserializer.Deserialize(merging_parser);
        
        _logger.Trace($"Deserialized configuration:\n{JObject.FromObject(config)}");
        return config;
    }
}