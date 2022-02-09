using System.Threading.Tasks;
using NLog;

namespace l99.driver.@base
{
    public class Handler
    {
        protected ILogger logger;
        protected Machine machine;
        
        public Machine Machine
        {
            get => machine;
        }
        
        public Handler(Machine machine, dynamic cfg)
        {
            logger = LogManager.GetLogger(this.GetType().FullName);
            this.machine = machine;
        }
        
        public virtual async Task<dynamic?> CreateAsync()
        {
            return null;
        }
        
        public async Task OnDataArrivalInternalAsync(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = await beforeDataArrivalAsync(veneers, veneer);
            dynamic? onRet = await OnDataArrivalAsync(veneers, veneer, beforeRet);
            await afterDataArrivalAsync(veneers, veneer, onRet);
        }

        protected virtual async Task<dynamic?> beforeDataArrivalAsync(Veneers veneers, Veneer veneer)
        {
            //await veneers.Machine.Broker.AddDiscoAsync(veneers.Machine.Id);
            
            return null;
        }
        
        public virtual async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            
            return null;
        }
        
        protected virtual async Task afterDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? onArrival)
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
            //await veneers.Machine.Broker.AddDiscoAsync(veneers.Machine.Id);
            
            return null;
        }
        
        public virtual async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            
            return null;
        }
        
        protected virtual async Task afterDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? onChange)
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
            
            return null;
        }
        
        public virtual async Task<dynamic?> OnErrorAsync(Veneers veneers, Veneer veneer, dynamic? beforeError)
        {
            
            return null;
        }
        
        protected virtual async Task afterDataErrorAsync(Veneers veneers, Veneer veneer, dynamic? onError)
        {
            
        }

        public virtual async Task OnStrategySweepCompleteInternalAsync()
        { 
            dynamic? beforeRet = await beforeSweepCompleteAsync(machine);
            dynamic? onRet = await OnStrategySweepCompleteAsync(machine, beforeRet);
            await afterSweepCompleteAsync(machine, onRet);
        }
        
        protected virtual async Task<dynamic?> beforeSweepCompleteAsync(Machine machine)
        {
            
            return null;
        }
        
        public virtual async Task<dynamic?> OnStrategySweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            
            return null;
        }

        protected virtual async Task afterSweepCompleteAsync(Machine machine, dynamic? onSweepComplete)
        {
            
        }

        public virtual async Task OnGenerateIntermediateModel(string json)
        {
            
        }
    }
}