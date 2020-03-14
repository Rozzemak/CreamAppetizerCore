using System.Collections.Generic;
using System.Threading.Tasks;

namespace CreamAppetizerCore.Model.Services.Singletons.Interfaces
{
    public interface ISteamLib
    {
        public Task<Dictionary<string, string>> GetSteamLibrariesAsync();
    }
}