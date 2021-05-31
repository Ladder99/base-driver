﻿using System;
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

        private DeviceStateEnum _device_state = DeviceStateEnum.NONE;
        public DeviceStateEnum DeviceState
        {
            get => _device_state;
        }
        
        //private string _broker_ip;
        //private int _broker_port;
        private string _namespace;
        private string _group_id;
        private string _edge_node_id;
        private string _device_id;
        private string _topicFormatNode = $"{{0}}/{{1}}/{{2}}/{{3}}";
        private string _topicFormatDevice = $"{{0}}/{{1}}/{{2}}/{{3}}/{{4}}";

        private int _seq;
        private int _bdSeq;
        //private IMqttClient _client;
        private Broker _broker;
        
        //public Protocol(string broker_ip, int broker_port, string group_id, string edge_node_id, string device_id, string @namespace = "spBv1.0")
        public Protocol(Broker broker, string group_id, string edge_node_id, string device_id, string @namespace = "spBv1.0")
        {
            //_broker_ip = broker_ip;
            //_broker_port = broker_port;
            _broker = broker;
            _group_id = group_id;
            _edge_node_id = edge_node_id;
            _device_id = device_id;
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
                return string.Format(_topicFormatDevice, _namespace, _group_id, messageType.ToString(), _edge_node_id, _device_id); 
            }
            else if (new MessageTypeEnum[] {MessageTypeEnum.NBIRTH,MessageTypeEnum.NDEATH,MessageTypeEnum.NDATA,MessageTypeEnum.NCMD}.Contains(messageType))
            {
                return string.Format(_topicFormatNode, _namespace, _group_id, messageType.ToString(), _edge_node_id);
            }

            return null;
        }

        private Dictionary<string, MetricWrapper> _node_metrics = new Dictionary<string, MetricWrapper>();
        private Dictionary<string, MetricWrapper> _device_metrics = new Dictionary<string, MetricWrapper>();
        
        public void add_node_metric(string name, dynamic value, MetricTypeEnum type = MetricTypeEnum.STRING)
        {
            if (_node_metrics.ContainsKey(name))
            {
                _node_metrics[name].metric.timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                _node_metrics[name].metric.value = value;
                _node_metrics[name].processed = false;
            }
            else
            {
                _node_metrics.Add(name, new MetricWrapper()
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
                type = type_to_enum<MetricTypeEnum>(value.GetType());

            if (_device_metrics.ContainsKey(name))
            {
                _device_metrics[name].metric.timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                _device_metrics[name].metric.value = value;
                _device_metrics[name].processed = false;
            }
            else
            {
                _device_metrics.Add(name, new MetricWrapper()
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

        public async Task give_node_birth()
        { 
            await create_client();
        }

        private async Task create_client()
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
            await dequeue_node_metrics(MessageTypeEnum.NBIRTH);
        }

        public async Task give_device_birth()
        {
            await dequeue_device_metrics(MessageTypeEnum.DBIRTH);
            _device_state = DeviceStateEnum.ALIVE;
        }

        public async Task give_device_death()
        {
            await dequeue_device_metrics(MessageTypeEnum.DDEATH);
            _device_state = DeviceStateEnum.DEAD;
        }

        public async Task dequeue_node_metrics(MessageTypeEnum msgType = MessageTypeEnum.NDATA)
        {
            var topic = formatTopic(msgType);

            var metrics = _node_metrics
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
            _node_metrics.ForEach(kv => kv.Value.processed = true);
        }
        
        public async Task dequeue_device_metrics(MessageTypeEnum msgType = MessageTypeEnum.DDATA)
        {
            var topic = formatTopic(msgType);

            var metrics = _device_metrics
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
            _device_metrics.ForEach(kv => kv.Value.processed = true);
        }
        
        public DataSet object_to_dataset(dynamic value)
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
                types.Add(type_to_enum<DataSetTypeEnum>(pv.GetType()));
                
                dvs.Add(new DataSet.DataSetValue
                {
                    value = pv
                });
            }

            ds.types = types.ToArray();
            ds.rows[0].elements = dvs.ToArray();
            return ds;
        }
        
        public DataSet array_to_dataset(dynamic value)
        {
            DataSet ds = new DataSet();
            
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
                        types.Add(type_to_enum<DataSetTypeEnum>(pv.GetType()));
                    }
                    else
                    {
                        var te = type_to_enum<DataSetTypeEnum>(pv.GetType());
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
        
        private TEnum type_to_enum<TEnum>(Type type) where TEnum: struct
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