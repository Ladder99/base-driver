using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace l99.driver.@base
{
    public class Handler
    {
        protected Machine machine;
        
        public Handler(Machine machine)
        {
            this.machine = machine;
        }
        
        public virtual void Initialize()
        {
            
        }
        
        public void OnDataArrivalInternal(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = beforeDataArrival(veneers, veneer);
            dynamic? onRet = OnDataArrival(veneers, veneer, beforeRet);
            afterDataArrival(veneers, veneer, onRet);
        }

        protected virtual dynamic? beforeDataArrival(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public virtual dynamic? OnDataArrival(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            return null;
        }
        
        protected virtual void afterDataArrival(Veneers veneers, Veneer veneer, dynamic? onArrival)
        {
            
        }
        
        public async Task OnDataArrivalInternalAsync(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = await beforeDataArrivalAsync(veneers, veneer);
            dynamic? onRet = await OnDataArrivalAsync(veneers, veneer, beforeRet);
            await afterDataArrivalAsync(veneers, veneer, onRet);
        }

        protected virtual async Task<dynamic?> beforeDataArrivalAsync(Veneers veneers, Veneer veneer)
        {
            Task.Yield();
            return null;
        }
        
        public virtual async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            Task.Yield();
            return null;
        }
        
        protected virtual async Task afterDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? onArrival)
        {
            
        }
        
        public virtual void OnDataChangeInternal(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = beforeDataChange(veneers, veneer);
            dynamic? onRet = OnDataChange(veneers, veneer, beforeRet);
            afterDataChange(veneers, veneer, onRet);
        }
        
        protected virtual dynamic? beforeDataChange(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public virtual dynamic? OnDataChange(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            return null;
        }
        
        protected virtual void afterDataChange(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            
        }
        
        public virtual async Task OnDataChangeInternalAsync(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = await beforeDataChangeAsync(veneers, veneer);
            dynamic? onRet = await OnDataChangeAsync(veneers, veneer, beforeRet);
            await afterDataChangeAsync(veneers, veneer, onRet);
        }
        
        protected virtual async Task<dynamic?> beforeDataChangeAsync(Veneers veneers, Veneer veneer)
        {
            Task.Yield();
            return null;
        }
        
        public virtual async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            Task.Yield();
            return null;
        }
        
        protected virtual async Task afterDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            
        }
        
        public virtual void OnErrorInternal(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = beforeDataError(veneers, veneer);
            dynamic? onRet = OnError(veneers, veneer, beforeRet);
            afterDataError(veneers, veneer, onRet);
        }
        
        protected virtual dynamic? beforeDataError(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public virtual dynamic? OnError(Veneers veneers, Veneer veneer, dynamic? beforeError)
        {
            return null;
        }
        
        protected virtual void afterDataError(Veneers veneers, Veneer veneer, dynamic? onError)
        {
            
        }
        
        public virtual async Task OnErrorInternalAsync(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = await beforeDataErrorAsync(veneers, veneer);
            dynamic? onRet = await OnErrorAsync(veneers, veneer, beforeRet);
            await afterDataErrorAsync(veneers, veneer, onRet);
        }
        
        protected virtual async Task<dynamic?> beforeDataErrorAsync(Veneers veneers, Veneer veneer)
        {
            Task.Yield();
            return null;
        }
        
        public virtual async Task<dynamic?> OnErrorAsync(Veneers veneers, Veneer veneer, dynamic? beforeError)
        {
            return null;
        }
        
        protected virtual async Task afterDataErrorAsync(Veneers veneers, Veneer veneer, dynamic? onError)
        {
            
        }

        public virtual void OnCollectorSweepCompleteInternal()
        { 
            dynamic? beforeRet = beforeSweepComplete(machine);
            dynamic? onRet = OnCollectorSweepComplete(machine, beforeRet);
            afterSweepComplete(machine, onRet);
        }

        protected virtual dynamic? beforeSweepComplete(Machine machine)
        {
            return null;
        }
        
        public virtual dynamic? OnCollectorSweepComplete(Machine machine, dynamic? beforeSweepComplete)
        {
            return null;
        }

        protected virtual void afterSweepComplete(Machine machine, dynamic? onSweepComplete)
        {
            
        }
        
        public virtual async Task OnCollectorSweepCompleteInternalAsync()
        { 
            dynamic? beforeRet = await beforeSweepCompleteAsync(machine);
            dynamic? onRet = await OnCollectorSweepCompleteAsync(machine, beforeRet);
            await afterSweepCompleteAsync(machine, onRet);
        }
        
        protected virtual async Task<dynamic?> beforeSweepCompleteAsync(Machine machine)
        {
            Task.Yield();
            return null;
        }
        
        public virtual async Task<dynamic?> OnCollectorSweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            Task.Yield();
            return null;
        }

        protected virtual async Task afterSweepCompleteAsync(Machine machine, dynamic? onSweepComplete)
        {
            Task.Yield();
        }
    }
}