using MrCMS.Services;
using Newtonsoft.Json;

namespace MrCMS.Helpers
{
    public static class AppDataSerializationExtensions
    {
        public static string Serialize(this IStoredInAppData storedInAppData)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new WritablePropertiesOnlyResolver()
            };
            return JsonConvert.SerializeObject(storedInAppData, settings);
        }
    }
}