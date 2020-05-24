using Newtonsoft.Json;

namespace ConsoleApp.Extensions
{
    public static class ObjectExtension
    {
        public static T DeepClone<T>(this T source) where T: new()
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
        }
    }
}