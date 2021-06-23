using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;

namespace l99.driver.@base.mqtt.sparkplugb
{
    public class Protocol
    {
        private class MetricWrapper
        {
            public bool processed;
            public Metric metric;
        }
        
        public enum DeviceStateEnum
        {
            NONE,
            ALIVE,
            DEAD
        }

        private DeviceStateEnum _deviceState = DeviceStateEnum.NONE;
        public DeviceStateEnum DeviceState
        {
            get => _deviceState;
        }
        
        //private string _broker_ip;
        //private int _broker_port;
        private string _namespace;
        private string _groupId;
        private string _edgeNodeId;
        private string _deviceId;
        private string _topicFormatNode = $"{{0}}/{{1}}/{{2}}/{{3}}";
        private string _topicFormatDevice = $"{{0}}/{{1}}/{{2}}/{{3}}/{{4}}";

        private int _seq;
        private int _bdSeq;
        //private IMqttClient _client;
        private Broker _broker;
        
        //public Protocol(string broker_ip, int broker_port, string group_id, string edge_node_id, string device_id, string @namespace = "spBv1.0")
        public Protocol(Broker broker, string groupId, string edgeNodeId, string deviceId, string @namespace = "spBv1.0")
        {
            //_broker_ip = broker_ip;
            //_broker_port = broker_port;
            _broker = broker;
            _groupId = groupId;
            _edgeNodeId = edgeNodeId;
            _deviceId = deviceId;
            _namespace = @namespace;
        }

        public string formatTopic(MessageTypeEnum messageType)
        {
            if (messageType == MessageTypeEnum.STATE)
            {
                return "STATE/unknown_scada_host_id";
            }
            else if (new MessageTypeEnum[] {MessageTypeEnum.DBIRTH,MessageTypeEnum.DDEATH,MessageTypeEnum.DDATA,MessageTypeEnum.DCMD}.Contains(messageType))
            {
                return string.Format(_topicFormatDevice, _namespace, _groupId, messageType.ToString(), _edgeNodeId, _deviceId); 
            }
            else if (new MessageTypeEnum[] {MessageTypeEnum.NBIRTH,MessageTypeEnum.NDEATH,MessageTypeEnum.NDATA,MessageTypeEnum.NCMD}.Contains(messageType))
            {
                return string.Format(_topicFormatNode, _namespace, _groupId, messageType.ToString(), _edgeNodeId);
            }

            return null;
        }

        private Dictionary<string, MetricWrapper> _nodeMetrics = new Dictionary<string, MetricWrapper>();
        private Dictionary<string, MetricWrapper> _deviceMetrics = new Dictionary<string, MetricWrapper>();
        
        public void add_node_metric(string name, dynamic value, MetricTypeEnum type = MetricTypeEnum.STRING)
        {
            if (_nodeMetrics.ContainsKey(name))
            {
                _nodeMetrics[name].metric.timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                _nodeMetrics[name].metric.value = value;
                _nodeMetrics[name].processed = false;
            }
            else
            {
                _nodeMetrics.Add(name, new MetricWrapper()
                {
                    processed = false,
                    metric = new Metric()
                    {
                        timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                        value = value,
                        name = name,
                        dataType = type
                    }
                });
            }
        }
        
        public void add_device_metric(string name, dynamic value, MetricTypeEnum type = MetricTypeEnum.UNKNOWN)
        {
            if (type == MetricTypeEnum.UNKNOWN)
                type = typeToEnum<MetricTypeEnum>(value.GetType());

            if (_deviceMetrics.ContainsKey(name))
            {
                _deviceMetrics[name].metric.timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                _deviceMetrics[name].metric.value = value;
                _deviceMetrics[name].processed = false;
            }
            else
            {
                _deviceMetrics.Add(name, new MetricWrapper()
                {
                    processed = false,
                    metric = new Metric()
                    {
                        timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                        value = value,
                        name = name,
                        dataType = type
                    }
                });
            }
        }
        
        private int seqNext()
        {
            if (_seq > 255)
                _seq = 0;

            int ns = _seq;
            _seq++;
            return ns;
        }

        private int seqCurrent()
        {
            return _seq - 1;
        }

        public async Task GiveNodeBirth()
        { 
            await createClient();
        }

        private async Task createClient()
        {
            ++_bdSeq;
            add_node_metric("bdSeq", _bdSeq, MetricTypeEnum.UINT64);
            /*
            var factory = new MqttFactory();
            var lwt = new MqttApplicationMessageBuilder()
                .WithTopic(formatTopic(MessageTypeEnum.NDEATH))
                .WithPayload(_bdSeq.ToString())
                .Build();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_broker_ip, _broker_port)
                .WithWillMessage(lwt)
                .Build();
            _client = factory.CreateMqttClient();
            var c = await _client.ConnectAsync(options);
            */
            await _broker.ConnectAsync(formatTopic(MessageTypeEnum.NDEATH), _bdSeq.ToString());
            await DequeueNodeMetrics(MessageTypeEnum.NBIRTH);
        }

        public async Task GiveDeviceBirth()
        {
            await DequeueDeviceMetrics(MessageTypeEnum.DBIRTH);
            _deviceState = DeviceStateEnum.ALIVE;
        }

        public async Task GiveDeviceDeath()
        {
            await DequeueDeviceMetrics(MessageTypeEnum.DDEATH);
            _deviceState = DeviceStateEnum.DEAD;
        }

        public async Task DequeueNodeMetrics(MessageTypeEnum msgType = MessageTypeEnum.NDATA)
        {
            var topic = formatTopic(msgType);

            var metrics = _nodeMetrics
                .Where(kv => kv.Value.processed == false)
                .Select(kv => kv.Value.metric);

            if (!metrics.Any() && msgType == MessageTypeEnum.NDATA)
                return;
            
            dynamic payload = new
            {
                timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                metrics,
                seq = seqNext()
            };
            
            //var msg = new MqttApplicationMessageBuilder()
            //    .WithTopic(topic)
            //    .WithPayload(JObject.FromObject(payload).ToString())
            //    .Build();
                
            //await _client.PublishAsync(msg, CancellationToken.None);
            
            await _broker.PublishAsync(topic, JObject.FromObject(payload).ToString(), false);
            _nodeMetrics.ForEach(kv => kv.Value.processed = true);
        }
        
        public async Task DequeueDeviceMetrics(MessageTypeEnum msgType = MessageTypeEnum.DDATA)
        {
            var topic = formatTopic(msgType);

            var metrics = _deviceMetrics
                .Where(kv => kv.Value.processed == false)
                .Select(kv => kv.Value.metric);

            if (!metrics.Any() && msgType == MessageTypeEnum.DDATA)
                return;
            
            dynamic payload = new
            {
                timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                metrics,
                seq = seqNext()
            };
            
            //var msg = new MqttApplicationMessageBuilder()
            //    .WithTopic(topic)
            //    .WithPayload(JObject.FromObject(payload).ToString())
            //    .Build();
                
            //await _client.PublishAsync(msg, CancellationToken.None);
            
            await _broker.PublishAsync(topic, JObject.FromObject(payload).ToString(), false);
            _deviceMetrics.ForEach(kv => kv.Value.processed = true);
        }
        
        public DataSet ObjectToDataset(dynamic value)
        {
            DataSet ds = new DataSet();
            PropertyInfo[] properties = value.GetType().GetProperties();
            ds.columns = properties.Select(p => p.Name).ToArray();
            ds.num_of_columns = properties.Length;
            //ds.types = Enumerable.Repeat(DataSetTypeEnum.STRING, properties.Length).ToArray();
            ds.rows = Enumerable.Repeat(new DataSet.DataSetRow(), 1).ToArray();
            
            List<DataSet.DataSetValue> dvs = new List<DataSet.DataSetValue>();
            List<DataSetTypeEnum> types = new List<DataSetTypeEnum>();
            
            foreach (var property in properties)
            {
                var pv = property.GetValue(value);
                types.Add(typeToEnum<DataSetTypeEnum>(pv.GetType()));
                
                dvs.Add(new DataSet.DataSetValue
                {
                    value = pv
                });
            }

            ds.types = types.ToArray();
            ds.rows[0].elements = dvs.ToArray();
            return ds;
        }
        
        public DataSet ArrayToDataset(dynamic value)
        {
            DataSet ds = new DataSet();

            if (value.Count == 0)
                return ds;
            
            //columns
            PropertyInfo[] properties = value[0].GetType().GetProperties();
            ds.columns = properties.Select(p => p.Name).ToArray();
            ds.num_of_columns = properties.Length;
            //ds.types = Enumerable.Repeat(DataSetTypeEnum.STRING, properties.Length).ToArray();
            
            //rows
            ds.rows = Enumerable.Range(1, (int)value.Count).Select(i => new DataSet.DataSetRow()).ToArray();
            
            int rc = 0;
            foreach (var obj in value)
            {
                List<DataSetTypeEnum> types = new List<DataSetTypeEnum>();
                PropertyInfo[] row_properties = value[rc].GetType().GetProperties();
                List<DataSet.DataSetValue> dvs = new List<DataSet.DataSetValue>();

                int pc = 0;
                foreach (var property in row_properties)
                {
                    var pv = property.GetValue(value[rc]);
                    
                    if (ds.types == null)
                    {
                        types.Add(typeToEnum<DataSetTypeEnum>(pv.GetType()));
                    }
                    else
                    {
                        var te = typeToEnum<DataSetTypeEnum>(pv.GetType());
                        if (te != ds.types[pc])
                            ds.types[pc] = DataSetTypeEnum.UNKNOWN;
                    }

                    dvs.Add(new DataSet.DataSetValue
                    {
                        value = pv
                    });

                    pc++;
                }

                if (ds.types == null)
                    ds.types = types.ToArray();
                
                ds.rows[rc].elements = dvs.ToArray();
                rc++;
            }

            return ds;
        }
        
        private TEnum typeToEnum<TEnum>(Type type) where TEnum: struct
        {
            TEnum t = default(TEnum);
            if (Enum.TryParse(type.Name, true, out t))
                return t;

            return default(TEnum);
        }
    }
    
    [AttributeUsage(validOn: AttributeTargets.Field, AllowMultiple = true)]
    public class RequiredAttribute: Attribute
    {
        public RequiredAttribute(MessageTypeEnum messageType)
        {
        }
    }
    
    public enum MessageTypeEnum
    {
        NBIRTH,
        NDEATH,
        DBIRTH,
        DDEATH,
        NDATA,
        DDATA,
        NCMD,
        DCMD,
        STATE
    }
    
    public enum MetricTypeEnum
    {
        UNKNOWN = 0,
        INT8,
        INT16,
        INT32,
        INT64,
        UINT8,
        UINT16,
        UINT32,
        UINT64,
        FLOAT,
        DOUBLE,
        BOOLEAN,
        STRING,
        DATETIME,
        TEXT,
        UUID,
        DATASET,
        BYTES,
        FILE,
        TEMPLATE = 19
    }
    
    public enum PropertyValueTypeEnum
    {
        UNKNOWN = 0,
        INT8,
        INT16,
        INT32,
        INT64,
        UINT8,
        UINT16,
        UINT32,
        UINT64,
        FLOAT,
        DOUBLE,
        BOOLEAN,
        STRING,
        DATETIME,
        TEXT = 14,
        PROPERTYSET = 20,
        PROPERTYSETLIST = 21
    }
    
    public enum DataSetTypeEnum
    {
        UNKNOWN = 0,
        INT8,
        INT16,
        INT32,
        INT64,
        UINT8,
        UINT16,
        UINT32,
        UINT64,
        FLOAT,
        DOUBLE,
        BOOLEAN,
        STRING,
        DATETIME,
        TEXT = 14
    }
    
    public enum TemplateParameterTypeEnum
    {
        UNKNOWN = 0,
        INT8,
        INT16,
        INT32,
        INT64,
        UINT8,
        UINT16,
        UINT32,
        UINT64,
        FLOAT,
        DOUBLE,
        BOOLEAN,
        STRING,
        DATETIME,
        TEXT = 14
    }

    public class Payload
    {
        public long timestamp;
        public Metric[] metrics;
        public long seq;
        //public string uuid;
        //public byte[] body;
    }

    public class Metric
    {
        [Required(MessageTypeEnum.NBIRTH)]
        [Required(MessageTypeEnum.DBIRTH)]
            public string name;
        
        //public long alias;
    
        public long timestamp;
        
        [Required(MessageTypeEnum.NBIRTH)]
        [Required(MessageTypeEnum.DBIRTH)]
            public MetricTypeEnum dataType;
        
        //public bool is_historical;
        
        //public bool is_transient;
        
        //public bool is_null;
        
        //public Metadata metadata;
        
        //public PropertySet properties;
        
        [Required(MessageTypeEnum.NBIRTH)]
        [Required(MessageTypeEnum.DBIRTH)]
            public dynamic value;
    }

    public class Metadata
    {
        public bool is_multi_part;
        public string content_type;
        public long size;
        public long seq;
        public string file_name;
        public string file_type;
        public string md5;
        public string description;
    }

    public class PropertySet
    {
        public string[] keys;
        public PropertyValue[] values;
    }

    public class PropertyValue
    {
        public PropertyValueTypeEnum type;
        public bool is_null;
        public dynamic value;
    }

    public class PropertySetList
    {
        public PropertySet[] propertyset;
    }

    public class DataSet
    {
        public long num_of_columns;
        public string[] columns;
        public DataSetTypeEnum[] types;
        public DataSetRow[] rows;
        
        public class DataSetRow
        {
            public DataSetValue[] elements;
        }
    
        public class DataSetValue
        {
            public dynamic value;
        }
    }

    public class Template
    {
        public string version;
        public dynamic[] metrics;
        public Parameter[] parameters;
        public string template_ref;
        public bool is_definition;
        
        public class Parameter
        {
            public string name;
            public TemplateParameterTypeEnum type;
            public dynamic value;
        }
    }
}