using System.Threading.Tasks;
using CreamAppetizerCore.Model.Services.Singletons.Interfaces;

namespace CreamAppetizerCore
{
    public class CreamAppetizer
    {
        public readonly ISteamLib SteamLib;

        public CreamAppetizer(ISteamLib steamLib)
        {
            SteamLib = steamLib;
        }

        public async Task RunAsync()
        {
            await this.SteamLib.GetSteamLibrariesAsync();
        }
    }
}