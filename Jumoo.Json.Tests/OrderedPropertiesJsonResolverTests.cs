using System.Text.Json;
using Jumoo.Json.Resolvers;

namespace Jumoo.Json.Tests
{
    public class OrderedPropertiesJsonResolverTests
    {
        private readonly JsonSerializerOptions _options;

        public OrderedPropertiesJsonResolverTests()
        {
            _options = new JsonSerializerOptions
            {
                TypeInfoResolver = new OrderedPropertiesJsonResolver(),
                WriteIndented = false,
            };
        }

        private class UnorderedObject
        {
            public string? Zebra { get; set; }
            public string? Apple { get; set; }
            public string? Mango { get; set; }
        }

        [Fact]
        public void Serialize_OrdersPropertiesAlphabetically()
        {
            var obj = new UnorderedObject { Zebra = "z", Apple = "a", Mango = "m" };

            var json = JsonSerializer.Serialize(obj, _options);

            var appleIdx = json.IndexOf("apple", StringComparison.OrdinalIgnoreCase);
            var mangoIdx = json.IndexOf("mango", StringComparison.OrdinalIgnoreCase);
            var zebraIdx = json.IndexOf("zebra", StringComparison.OrdinalIgnoreCase);

            Assert.True(appleIdx < mangoIdx);
            Assert.True(mangoIdx < zebraIdx);
        }

        [Fact]
        public void Serialize_NonObjectTypes_AreNotAffected()
        {
            var list = new List<int> { 3, 1, 2 };

            var json = JsonSerializer.Serialize(list, _options);

            Assert.Equal("[3,1,2]", json);
        }

        [Fact]
        public void GetTypeInfo_ReturnsValidTypeInfo()
        {
            var resolver = new OrderedPropertiesJsonResolver();
            var options = new JsonSerializerOptions();

            var typeInfo = resolver.GetTypeInfo(typeof(UnorderedObject), options);

            Assert.NotNull(typeInfo);
            Assert.Equal(typeof(UnorderedObject), typeInfo.Type);
        }
    }
}
