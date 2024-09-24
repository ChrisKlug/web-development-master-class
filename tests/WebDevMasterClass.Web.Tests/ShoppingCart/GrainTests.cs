using Orleans.TestingHost;
using WebDevMasterClass.Web.ShoppingCart;
using static WebDevMasterClass.Web.Tests.ShoppingCart.GrainTests;

namespace WebDevMasterClass.Web.Tests.ShoppingCart;

public sealed class ClusterFixture : IDisposable
{
    public TestCluster Cluster { get; } = new TestClusterBuilder()
                                            .AddSiloBuilderConfigurator<SiloConfigurator>()
                                            .Build();

    public ClusterFixture() => Cluster.Deploy();

    void IDisposable.Dispose() => Cluster.StopAllSilos();

    private class SiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder hostBuilder) => hostBuilder.AddMemoryGrainStorageAsDefault();
    }
}

[CollectionDefinition(Name)]
public sealed class ClusterCollection : ICollectionFixture<ClusterFixture>
{
    public const string Name = nameof(ClusterCollection);
}

public class GrainTests
{
    [Collection(ClusterCollection.Name)]
    public class ShoppingCartGrain(ClusterFixture fixture)
    {
        [Fact]
        public async Task Is_empty_when_created()
        {
            var grain = fixture.Cluster.GrainFactory.GetGrain<IShoppingCartGrain>("TestCart");
            
            var items = await grain.GetItems();

            Assert.Empty(items);
        }

        [Fact]
        public async Task Returns_items_that_have_been_added()
        {
            var grain = fixture.Cluster.GrainFactory.GetGrain<IShoppingCartGrain>("AddItemsCart");

            await grain.AddItem(new ShoppingCartItem
            {
                ProductId = 1,
                ProductName = "Test Product 1",
                Price = 10,
                Count = 1
            });
            await grain.AddItem(new ShoppingCartItem
            {
                ProductId = 2,
                ProductName = "Test Product 2",
                Price = 20,
                Count = 2
            });

            var items = await grain.GetItems();

            Assert.Contains(items, x => x.ProductId == 1
                                    && x.ProductName == "Test Product 1"
                                    && x.Price == 10
                                    && x.Count == 1);
            Assert.Contains(items, x => x.ProductId == 2
                                    && x.ProductName == "Test Product 2"
                                    && x.Price == 20
                                    && x.Count == 2);
        }
    }
}