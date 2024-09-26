using Bazinga.AspNetCore.Authentication.Basic;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WebDevMasterClass.Services.Products.Client;
using WebDevMasterClass.Web.ShoppingCart;

namespace WebDevMasterClass.Web.Test.ShoppingCart;

public class ShoppingCartTests
{
    public class GetShoppingCart
    {
        [Fact]
        public async Task Gets_empty_shopping_cart_by_default()
        {
            var client = GetClient();

            var response = await client.GetAsync("/api/shopping-cart");

            response.EnsureSuccessStatusCode();

            Assert.Equal("application/json", response.Content.Headers.ContentType!.MediaType);

            var shoppingCart = JArray.Parse(await response.Content.ReadAsStringAsync());

            Assert.Empty(shoppingCart);
        }

        [Fact]
        public async Task Gets_shopping_cart_from_grain_if_ShoppingCartId_cookie_exists()
        {
            var grainFake = A.Fake<IShoppingCartGrain>();
            A.CallTo(() => grainFake.GetItems()).Returns([ new ShoppingCartItem {
                    ProductId = 1,
                    ProductName = "Test Product",
                    Count = 1,
                    Price = 1.23m
                } ]);

            var client = GetClient(x =>
            {
                A.CallTo(() => x.GetGrain<IShoppingCartGrain>("TestCart", null)).Returns(grainFake);
            });

            client.DefaultRequestHeaders.Add("Cookie", "ShoppingCartId=TestCart");
            var response = await client.GetAsync("/api/shopping-cart");

            response.EnsureSuccessStatusCode();

            Assert.Equal("application/json", response.Content.Headers.ContentType!.MediaType);

            var shoppingCart = JArray.Parse(await response.Content.ReadAsStringAsync());

            var item = (JObject)Assert.Single(shoppingCart);

            Assert.Equal(1, item["productId"]);
            Assert.Equal("Test Product", item["productName"]);
            Assert.Equal(1, item["count"]);
            Assert.Equal(1.23m, item["price"]);
        }
    }

    public class AddShoppingCartItem
    {
        private static Product TestProduct = new Product(1, "Test Product", "A Test Product", 1.23m, false, "thumbnail.jpg", "image.jpg");

        [Fact]
        public async Task Adds_item_to_grain_with_random_id_if_no_cookie_exists()
        {
            var grainFake = A.Fake<IShoppingCartGrain>();

            var client = GetClient(grainFactory =>
            {
                A.CallTo(() => grainFactory.GetGrain<IShoppingCartGrain>(A<string>.That.Matches(x => x.Length == 30), null)).Returns(grainFake);
            },
            productsClient =>
            {
                A.CallTo(() => productsClient.GetProduct(1)).Returns(TestProduct);
            });

            var response = await client.PostAsJsonAsync("/api/shopping-cart", new { ProductId = 1, Count = 1 });

            response.EnsureSuccessStatusCode();

            A.CallTo(() => grainFake.AddItem(A<ShoppingCartItem>.That
                                    .Matches(x => x.ProductId == 1 && x.Price == 1.23m && x.Count == 1)))
                                    .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Adds_item_to_grain_with_id_from_ShoppingCartId_cookie()
        {
            var grainFake = A.Fake<IShoppingCartGrain>();

            var client = GetClient(grainFactory =>
            {
                A.CallTo(() => grainFactory.GetGrain<IShoppingCartGrain>("TestCart", null)).Returns(grainFake);
            },
            productsClient =>
            {
                A.CallTo(() => productsClient.GetProduct(1)).Returns(TestProduct);
            });

            client.DefaultRequestHeaders.Add("Cookie", "ShoppingCartId=TestCart");
            var response = await client.PostAsJsonAsync("/api/shopping-cart", new { ProductId = 1, Count = 1 });

            response.EnsureSuccessStatusCode();

            A.CallTo(() => grainFake.AddItem(A<ShoppingCartItem>
                                    .That.Matches(x => x.ProductId == 1 && x.Price == 1.23m && x.Count == 1)))
                                    .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Adds_ShoppingCartId_cookie_if_no_cookie_exists()
        {
            var grainFake = A.Fake<IShoppingCartGrain>();

            var client = GetClient(grainFactory =>
            {
                A.CallTo(() => grainFactory.GetGrain<IShoppingCartGrain>(A<string>.Ignored, null)).Returns(grainFake);
            },
            productsClient =>
            {
                A.CallTo(() => productsClient.GetProduct(1)).Returns(TestProduct);
            });

            var response = await client.PostAsJsonAsync("/api/shopping-cart", new { ProductId = 1, Count = 1 });

            response.EnsureSuccessStatusCode();

            Assert.True(response.Headers.TryGetValues("Set-Cookie", out var values));
            Assert.Contains(values, x => x.StartsWith("ShoppingCartId="));
        }

        [Fact]
        public async Task Returns_all_items_in_ShoppingCart()
        {
            var grainFake = A.Fake<IShoppingCartGrain>();

            A.CallTo(() => grainFake.GetItems()).Returns([ new ShoppingCartItem {
                    ProductId = 1,
                    ProductName = "Test Product",
                    Count = 1,
                    Price = 1.23m
                } ]);


            var client = GetClient(grainFactory =>
            {
                A.CallTo(() => grainFactory.GetGrain<IShoppingCartGrain>(A<string>.Ignored, null)).Returns(grainFake);
            },
            productsClient =>
            {
                A.CallTo(() => productsClient.GetProduct(1)).Returns(TestProduct);
            });

            var response = await client.PostAsJsonAsync("/api/shopping-cart", new { ProductId = 1, Count = 1 });

            response.EnsureSuccessStatusCode();

            Assert.Equal("application/json", response.Content.Headers.ContentType!.MediaType);

            var shoppingCart = JArray.Parse(await response.Content.ReadAsStringAsync());

            var item = (JObject)Assert.Single(shoppingCart);

            Assert.Equal(1, item["productId"]);
            Assert.Equal("Test Product", item["productName"]);
            Assert.Equal(1, item["count"]);
            Assert.Equal(1.23m, item["price"]);
        }
    }

    public class Me
    {
        [Fact]
        public async Task Returns_HTTP_401_if_not_authenticated()
        {
            var client = GetClient();

            var response = await client.GetAsync("/api/me");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    private static HttpClient GetClient(Action<IGrainFactory>? configureGrainFactory = null,
                                        Action<IProductsClient>? configureProductsClient = null)
    {
        var app = new WebApplicationFactory<Program>()
                        .WithWebHostBuilder(builder =>
                        {
                            builder.UseEnvironment("IntegrationTesting");

                            builder.ConfigureTestServices(services =>
                            {
                                var grainFactoryFake = A.Fake<IGrainFactory>();
                                configureGrainFactory?.Invoke(grainFactoryFake);
                                services.AddSingleton(grainFactoryFake);

                                var productsClientFake = A.Fake<IProductsClient>();
                                configureProductsClient?.Invoke(productsClientFake);
                                services.AddSingleton(productsClientFake);

                                services.AddAuthentication(options => {
                                    options.DefaultScheme = BasicAuthenticationDefaults.AuthenticationScheme;
                                    options.DefaultChallengeScheme = BasicAuthenticationDefaults.AuthenticationScheme;
                                })
                                        .AddBasicAuthentication(creds => 
                                            Task.FromResult(creds.username.Equals("test", StringComparison.InvariantCultureIgnoreCase) 
                                            && creds.password == "test"));
                            });
                        });
        var client = app.CreateClient();

        return client;
    }
}
