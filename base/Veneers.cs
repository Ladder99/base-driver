using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace l99.driver.@base
{
    public class Veneers
    {
        public Machine Machine
        {
            get { return _machine; }
        }
        
        private Machine _machine;

        public Func<Veneers, Veneer, Task> OnDataArrivalAsync = async (vv, v) => {  };
        
        public Func<Veneers, Veneer, Task> OnDataChangeAsync = async (vv, v) => {  };
        
        public Func<Veneers, Veneer, Task> OnErrorAsync = async (vv, v) => {  };
        
        private string SPLIT_SEP = "/";
        
        private List<Veneer> _wholeVeneers = new List<Veneer>();
        
        private Dictionary<dynamic, List<Veneer>> _slicedVeneers = new Dictionary<dynamic, List<Veneer>>();
        
        public Veneers(Machine machine)
        {
            _machine = machine;
        }
        
        public void Slice(dynamic split)
        {
            foreach (var key in split)
            {
                _slicedVeneers[key] = new List<Veneer>();
            }
        }
        
        public void Slice(dynamic sliceKey, dynamic split)
        {
            foreach (var key in split)
            {
                _slicedVeneers[sliceKey+SPLIT_SEP+key] = new List<Veneer>();
            }
        }

        public void Add(Type veneerType, string name, bool isCompound = false, bool isInternal = false)
        {
            Veneer veneer = (Veneer)Activator.CreateInstance(veneerType, new object[] { name, isCompound, isInternal });
            veneer.OnArrivalAsync = async (v) => await OnDataArrivalAsync(this, v);
            veneer.OnChangeAsync = async (v) => await OnDataChangeAsync(this, v);
            veneer.OnErrorAsync = async (v) => await OnErrorAsync(this, v);
            _wholeVeneers.Add(veneer);
        }

        public void AddAcrossSlices(Type veneerType, string name, bool isCompound = false, bool isInternal = false)
        {
            foreach (var key in _slicedVeneers.Keys)
            {
                Veneer veneer = (Veneer)Activator.CreateInstance(veneerType, new object[] { name, isCompound, isInternal });
                veneer.SetSliceKey(key);
                veneer.OnArrivalAsync = async (v) => await OnDataArrivalAsync(this, v);
                veneer.OnChangeAsync = async (v) => await OnDataChangeAsync(this, v);
                veneer.OnErrorAsync = async (v) => await OnErrorAsync(this, v);
                _slicedVeneers[key].Add(veneer);
            }
        }
        
        public void AddAcrossSlices(dynamic sliceKey, Type veneerType, string name, bool isCompound = false, bool isInternal = false)
        {
            foreach (var key in _slicedVeneers.Keys)
            {
                var key_parts = key.ToString().Split(SPLIT_SEP);
                if (key_parts.Length == 1)
                    continue;
                
                Veneer veneer = (Veneer)Activator.CreateInstance(veneerType, new object[] { name, isCompound, isInternal });
                veneer.SetSliceKey(key);
                veneer.OnArrivalAsync = async (v) => await OnDataArrivalAsync(this, v);
                veneer.OnChangeAsync = async (v) => await OnDataChangeAsync(this, v);
                veneer.OnErrorAsync = async (v) => await OnErrorAsync(this, v);
                _slicedVeneers[key].Add(veneer);
            }
        }

        public async Task<dynamic> PeelAsync(string name, dynamic input, params dynamic?[] additional_inputs)
        {
            return await _wholeVeneers.FirstOrDefault(v => v.Name == name).PeelAsync(input, additional_inputs);
        }
        
        public async Task<dynamic> PeelAcrossAsync(dynamic split, string name, dynamic input, params dynamic?[] additional_inputs)
        {
            foreach (var key in _slicedVeneers.Keys)
            {
                dynamic temp_split = split;

                if (split is Array)
                {
                    temp_split = string.Join(SPLIT_SEP, split);
                }
                
                if (key.Equals(temp_split))
                {
                    foreach (Veneer veneer in _slicedVeneers[key])
                    {
                        if (veneer.Name == name)
                        {
                            return await veneer.PeelAsync(input, additional_inputs);
                        }
                    }
                }
            }

            
            return new { };
        }

        public void Mark(dynamic split, dynamic marker)
        {
            foreach (var key in _slicedVeneers.Keys)
            {
                dynamic temp_split = split;

                if (split is Array)
                {
                    temp_split = string.Join(SPLIT_SEP, split);
                }
                
                if (key.Equals(temp_split))
                {
                    foreach (Veneer veneer in _slicedVeneers[key])
                    {
                        veneer.Mark(marker);
                    }
                }
            }
        }
    }
}