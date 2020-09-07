using Microsoft.Extensions.Configuration;
using System;

namespace Offer.API.Application.Helpers
{
    public static class EnvironmentUtils
    {
        private static IConfiguration _Configuration;
        public static IConfiguration Configuration
        {
            get
            {
                return _Configuration;
            }
            set
            {
                if (_Configuration == null)
                {
                    _Configuration = value;
                }
            }
        }

        public static string GetVariable(string environmentVariable, string configurationKey = null, string defaultValue = null)
        {
            string envVar = Environment.GetEnvironmentVariable(environmentVariable);
            if (String.IsNullOrEmpty(envVar) && !String.IsNullOrEmpty(configurationKey))
            {
                envVar = _Configuration[configurationKey];
            }
            return envVar ?? defaultValue;
        }
    }
}
