using System.Threading.Tasks;
using NLog;

namespace l99.driver.@base
{
    public class Transport
    {
        protected ILogger logger;
        public Machine Machine => machine;
        protected Machine machine;
        
        public Transport(Machine machine, dynamic cfg)
        {
            logger = LogManager.GetLogger(this.GetType().FullName);
            this.machine = machine;
        }

        public virtual async Task<dynamic?> CreateAsync()
        {
            return null;
        }
        
        public virtual async Task ConnectAsync()
        {
            
        }
        
        public virtual async Task SendAsync(params dynamic[] parameters)
        {
            
        }
        
        public virtual async Task OnGenerateIntermediateModelAsync(dynamic model)
        {
            
        }
    }
}