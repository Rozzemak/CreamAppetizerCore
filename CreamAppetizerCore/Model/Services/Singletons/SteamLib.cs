using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreamAppetizerCore.Model.Services.Base;
using CreamAppetizerCore.Model.Services.Singletons.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CreamAppetizerCore.Model.Services.Singletons
{
    public class SteamLib : BaseService, ISteamLib
    {
        public SteamLib(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<Dictionary<string, string>> GetSteamLibrariesAsync()
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<CreamAppetizer>>();
            logger.LogInformation("Well, DI works now");
            return new Dictionary<string, string>(){{"this", "works"}};
        }

        
    }
}