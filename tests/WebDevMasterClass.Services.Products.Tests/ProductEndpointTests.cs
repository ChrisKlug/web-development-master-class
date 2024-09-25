using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebDevMasterClass.Services.Products.Tests.Data;
using WebDevMasterClass.Services.Products.Tests.Infrastructure;

namespace WebDevMasterClass.Services.Products.Tests;

public class ProductEndpointTests
{
    [Fact]
    public Task GET_returns_HTTP_200_and_the_requested_product()
    {
        var productId = 0;
        return TestHelper.ExecuteTest(dbSetup: async cmd =>
        {
            productId = await cmd.AddProduct("Product 1", "Description 1", 100m, true, "product1");
        },
        test: async client =>
        {
            var response = await client.GetAsync($"/api/products/{productId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            dynamic json = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(productId, (int)json.id);
            Assert.Equal("Product 1", (string)json.name);
            Assert.Equal("Description 1", (string)json.description);
            Assert.Equal(100m, (decimal)json.price);
            Assert.True((bool)json.isFeatured);
            Assert.Equal("product1_thumbnail.jpg", (string)json.thumbnailUrl);
            Assert.Equal("product1.jpg", (string)json.imageUrl);
        });
    }
}
