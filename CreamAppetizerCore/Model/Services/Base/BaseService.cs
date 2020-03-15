using System;

namespace CreamAppetizerCore.Model.Services.Base
{
    public class BaseService
    {
        protected readonly IServiceProvider ServiceProvider;

        public BaseService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}