using Newtonsoft.Json;

namespace ConsoleApp.Extensions
{
    internal static class ObjectExtension
    {
        public static T DeepClone<T>(this T source) where T: new()
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, settings), settings);
        }
    }
}