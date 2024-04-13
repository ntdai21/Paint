using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Xml.Serialization;

namespace Contract
{
    public static class DeepCopyHelper
    {
        public static T DeepCopy<T>(T input)
        {
            var jsonString = JsonSerializer.Serialize(input);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
