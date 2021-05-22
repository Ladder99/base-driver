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
        
        public virtual async Task InitializeAsync()
        {
            await Task.Yield();
        }
        
        public async Task OnDataArrivalInternalAsync(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = await beforeDataArrivalAsync(veneers, veneer);
            dynamic? onRet = await OnDataArrivalAsync(veneers, veneer, beforeRet);
            await afterDataArrivalAsync(veneers, veneer, onRet);
        }

        protected virtual async Task<dynamic?> beforeDataArrivalAsync(Veneers veneers, Veneer veneer)
        {
            await Task.Yield();
            return null;
        }
        
        public virtual async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            await Task.Yield();
            return null;
        }
        
        protected virtual async Task afterDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? onArrival)
        {
            await Task.Yield();
        }
        
        public virtual async Task OnDataChangeInternalAsync(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = await beforeDataChangeAsync(veneers, veneer);
            dynamic? onRet = await OnDataChangeAsync(veneers, veneer, beforeRet);
            await afterDataChangeAsync(veneers, veneer, onRet);
        }
        
        protected virtual async Task<dynamic?> beforeDataChangeAsync(Veneers veneers, Veneer veneer)
        {
            await Task.Yield();
            return null;
        }
        
        public virtual async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            await Task.Yield();
            return null;
        }
        
        protected virtual async Task afterDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            await Task.Yield();
        }
        
        public virtual async Task OnErrorInternalAsync(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = await beforeDataErrorAsync(veneers, veneer);
            dynamic? onRet = await OnErrorAsync(veneers, veneer, beforeRet);
            await afterDataErrorAsync(veneers, veneer, onRet);
        }
        
        protected virtual async Task<dynamic?> beforeDataErrorAsync(Veneers veneers, Veneer veneer)
        {
            await Task.Yield();
            return null;
        }
        
        public virtual async Task<dynamic?> OnErrorAsync(Veneers veneers, Veneer veneer, dynamic? beforeError)
        {
            await Task.Yield();
            return null;
        }
        
        protected virtual async Task afterDataErrorAsync(Veneers veneers, Veneer veneer, dynamic? onError)
        {
            await Task.Yield();
        }

        public virtual async Task OnCollectorSweepCompleteInternalAsync()
        { 
            dynamic? beforeRet = await beforeSweepCompleteAsync(machine);
            dynamic? onRet = await OnCollectorSweepCompleteAsync(machine, beforeRet);
            await afterSweepCompleteAsync(machine, onRet);
        }
        
        protected virtual async Task<dynamic?> beforeSweepCompleteAsync(Machine machine)
        {
            await Task.Yield();
            return null;
        }
        
        public virtual async Task<dynamic?> OnCollectorSweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            await Task.Yield();
            return null;
        }

        protected virtual async Task afterSweepCompleteAsync(Machine machine, dynamic? onSweepComplete)
        {
            await Task.Yield();
        }
    }
}