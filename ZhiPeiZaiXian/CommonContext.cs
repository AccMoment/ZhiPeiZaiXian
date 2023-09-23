using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ZhiPeiZaiXian;
[JsonSerializable(typeof(Courses))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(List<Study>))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(Settings))]
[JsonSerializable(typeof(LoginParam))]
[JsonSerializable(typeof(EmptyBody))]
[JsonSerializable(typeof(ValidateCode))]
public partial class CommonContext : JsonSerializerContext
{
    
}