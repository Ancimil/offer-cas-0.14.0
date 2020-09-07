using MicroserviceCommon.ApiUtil.Serialization.KebabCase;
using Newtonsoft.Json;
using System;

namespace Offer.Infrastructure.Utils
{
    public class RepositoryUtil
    {

        public static string SerializeObjectWithoutNull(Object obj)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            settings.ContractResolver = KebabCasePropertyNameResolver.Instance;
            settings.Converters.Add(
                new Newtonsoft.Json.Converters.StringEnumConverter());

            return JsonConvert.SerializeObject(obj,
                                     Newtonsoft.Json.Formatting.None, settings
             );
        }
    }
}
