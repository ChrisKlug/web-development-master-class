namespace WebDevMasterClass.Web.ShoppingCart;

public interface IShoppingCartGrain : IGrainWithStringKey
{
    Task AddItem(ShoppingCartItem item);
    Task<ShoppingCartItem[]> GetItems();
    Task MigrateFrom(string grainId);
    Task Clear();
}

[GenerateSerializer]
public class ShoppingCartItem
{
    [Id(1)]public int ProductId { get; set; }
    [Id(2)]public string ProductName { get; set; } = string.Empty;
    [Id(3)]public decimal Price { get; set; }
    [Id(4)]public int Count { get; set; }
}

public class ShoppingCartGrain : Grain, IShoppingCartGrain
{
    private readonly IPersistentState<ShoppingCartState> state;
    private readonly IGrainFactory grainFactory;

    public ShoppingCartGrain([PersistentState("ShoppingCartState")]IPersistentState<ShoppingCartState> state, 
                                IGrainFactory grainFactory)
    {
        this.state = state;
        this.grainFactory = grainFactory;
    }

    public Task AddItem(ShoppingCartItem item)
    {
        var existingItem = state.State.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (existingItem is null)
        {
            state.State.Items.Add(item);
        }
        else
        {
            existingItem.Count += item.Count;
        }
        return state.WriteStateAsync();
    }

    public Task<ShoppingCartItem[]> GetItems()
        => Task.FromResult(state.State.Items.ToArray());

    public async Task MigrateFrom(string grainId)
    {
        var fromGrain = grainFactory.GetGrain<IShoppingCartGrain>(grainId);
        var items = await fromGrain.GetItems();

        foreach (var item in items)
        {
            var existingItem = state.State.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Count = item.Count;
            }
            else
            {
                state.State.Items.Add(item);
            }
        }

        await state.WriteStateAsync();
        await fromGrain.Clear();
    }

    public Task Clear()
    {
        DeactivateOnIdle();
        return state.ClearStateAsync();
    }

    public class ShoppingCartState
    {
        public List<ShoppingCartItem> Items { get; set; } = [];
    }
}
