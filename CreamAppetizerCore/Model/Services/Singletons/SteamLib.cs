using System.Collections.Generic;
using System.Threading.Tasks;
using CreamAppetizerCore.Model.Services.Singletons.Interfaces;

namespace CreamAppetizerCore.Model.Services.Singletons
{
    public class SteamLib : ISteamLib
    {
        public async Task<Dictionary<string, string>> GetSteamLibrariesAsync()
        {
            return new Dictionary<string, string>(){{"this", "works"}};
        }
    }
}