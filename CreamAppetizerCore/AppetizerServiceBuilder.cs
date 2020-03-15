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
            var serviceRefs = new List<string>();
            var host = new HostBuilder()
                .ConfigureServices(async (hostContext, services) =>
                {
                    services.AddLogging(configure => configure.AddConsole());
                    services.AddTransient<CreamAppetizer>();
                    
                    // InitSingletons
                    serviceRefs.AddRange((await InitServicesByNamespaceNameAsync("Singletons", services,
                        (collection, interfaceType, implType) => services.AddSingleton(interfaceType, implType))
                        .ConfigureAwait(false)).Item1.Select(type => type.Name));
                    // InitScopes
                    serviceRefs.AddRange((await InitServicesByNamespaceNameAsync("Scoped", services,
                        (collection, interfaceType, implType) => services.AddScoped(interfaceType, implType))
                        .ConfigureAwait(false)).Item1.Select(type => type.Name));
                    //services.AddSingleton<IDataAccessLayer, CDataAccessLayer>(); 
                }).Build();
            using var serviceScope = host.Services.CreateScope();
            var servicesProvider = serviceScope.ServiceProvider;
            try
            {
                var logger = servicesProvider.GetService<ILogger<CreamAppetizer>>();
                logger.LogInformation(nameof(AppetizerServiceBuilder) + $" successfully built host. Services: [{serviceRefs.Count()}] " 
                                                                      + $"=> [{string.Join(", ", serviceRefs)}]");
                var creamCoreService = servicesProvider.GetRequiredService<CreamAppetizer>(); 
                var creamRun = creamCoreService.RunAsync().ConfigureAwait(false);
                logger.LogInformation(nameof(CreamAppetizer) + " is running now.");
                await creamRun;
                //var autoAddedSingletonTest = servicesProvider.GetRequiredService<ISteamLib>();
                //var result = (await autoAddedSingletonTest.GetSteamLibrariesAsync().ConfigureAwait(false)).Select(
                //    pair => $"{pair.Key} {pair.Value}").ToArray();
                //logger.LogInformation(
                //    $"{nameof(autoAddedSingletonTest)} {string.Join("", result)}");
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
                .Where(info => (bool) info?.FullName?.ToLower()?.Contains(namespaceName.ToLower()) && (info.IsClass || info.IsInterface))
                .ToAsyncEnumerable().ToListAsync().
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