//namespace ASP.Claims.API.Settings;

//using Microsoft.Azure.Cosmos;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using System.IO;

//public class CosmosSystemTextJsonSerializer(JsonSerializerOptions options) : CosmosSerializer
//{
//    private readonly JsonSerializerOptions _options = options;

//    public override T FromStream<T>(Stream stream)
//    {
//        if (stream == null || stream.CanRead == false)
//        {
//            return default!;
//        }

//        if (typeof(Stream).IsAssignableFrom(typeof(T)))
//        {
//            return (T)(object)stream;
//        }

//        using (stream)
//        {
//            return JsonSerializer.DeserializeAsync<T>(stream, _options).GetAwaiter().GetResult()!;
//        }
//    }

//    public override Stream ToStream<T>(T input)
//    {
//        var stream = new MemoryStream();
//        JsonSerializer.SerializeAsync(stream, input, _options).GetAwaiter().GetResult();
//        stream.Position = 0;
//        return stream;
//    }
//}