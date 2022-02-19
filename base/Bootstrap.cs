using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace l99.driver.@base
{
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
                string config_file = getArgument(args, "--config", "config.yml");
                dynamic config = readConfig(config_file);
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
    
            static dynamic readConfig(string config_file)
            {
                var input = new StreamReader(config_file);

                var parser = new MergingParser(new Parser(input));
                
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
    
                var config = deserializer.Deserialize(parser);
                
                _logger.Trace($"Deserialized configuration:\n{JObject.FromObject(config).ToString()}");
                return config;
            }
        }
}