using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CreamAppetizerCore.Model.Services;
using CreamAppetizerCore.Model.Services.Singletons;
using CreamAppetizerCore.Model.Services.Singletons.Interfaces;
using Dasync.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CreamAppetizerCore
{
    public static class AppetizerServiceBuilder
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices(async (hostContext, services) =>
                {
                    services.AddLogging(configure => configure.AddConsole());
                    services.AddTransient<CreamAppetizer>();
                    
                    // InitSingletons
                    await InitServicesByNamespaceNameAsync("Singletons", services,
                        (collection, interfaceType, implType) => services.AddSingleton(interfaceType, implType))
                        .ConfigureAwait(false);
                    // InitScopes
                    await InitServicesByNamespaceNameAsync("Scoped", services,
                        (collection, interfaceType, implType) => services.AddScoped(interfaceType, implType))
                        .ConfigureAwait(false);
                    
                    //services.AddSingleton<IDataAccessLayer, CDataAccessLayer>(); 
                }).Build();
            using var serviceScope = host.Services.CreateScope();
            var servicesProvider = serviceScope.ServiceProvider;
            try
            {
                var creamCoreService = servicesProvider.GetRequiredService<CreamAppetizer>();
                await creamCoreService.Run().ConfigureAwait(false);
                var logger = servicesProvider.GetService<ILogger<CreamAppetizer>>();
                logger.LogInformation(nameof(AppetizerServiceBuilder) + " finished successfully init services.");
                var autoAddedSingletonTest = servicesProvider.GetRequiredService<ISteamLib>();
                var result = (await autoAddedSingletonTest.GetSteamLibrariesAsync().ConfigureAwait(false)).Select(
                    pair => $"{pair.Key} {pair.Value}").ToArray();
                logger.LogInformation(
                    $"{nameof(autoAddedSingletonTest)} {string.Join("", result)}");
            }
            catch (Exception ex)
            {
                var logger = servicesProvider.GetService<ILogger<CreamAppetizer>>();
                logger.LogCritical(ex.ToString());
            }
        }


        /// <summary>
        /// Adds namespace contained services and their impl. via provided func to provided collection. 
        /// </summary>
        /// <param name="namespaceName">Namespace name in which services are located</param>
        /// <param name="serviceCollection"></param>
        /// <param name="services">Builder service collection</param>
        /// <returns>tuple of added interfaces and their impl (not a dictionary style!)</returns>
        private static async Task<(IEnumerable<Type>, IEnumerable<Type>)> 
            InitServicesByNamespaceNameAsync(string namespaceName, IServiceCollection serviceCollection, Action<IServiceCollection, Type, Type> services)
        {
            var scopeTypes = await Assembly.GetAssembly(typeof(CreamAppetizer)).DefinedTypes
                .Where(info => (bool) info?.FullName?.ToLower()?.Contains(namespaceName.ToLower()) && info.IsClass || info.IsInterface)
                .Select(info => info).ToAsyncEnumerable().ToListAsync().
                ConfigureAwait(false);
            var nonInterfaceScopeTypes = scopeTypes.Where(type => !type.IsInterface).ToList();
            foreach (var type in scopeTypes.Where(type => type.IsInterface))
            {
                var implService = nonInterfaceScopeTypes.FirstOrDefault(type1 => type.Name.EndsWith(type1.Name));
                if(!(implService is null)) services(serviceCollection, type, implService);
            }
            return (scopeTypes.Where(type => type.IsInterface), nonInterfaceScopeTypes);
        }
    }
}