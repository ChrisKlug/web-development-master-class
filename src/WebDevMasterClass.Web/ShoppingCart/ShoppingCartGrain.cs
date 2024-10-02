namespace WebDevMasterClass.Web.ShoppingCart;

public interface IShoppingCartGrain : IGrainWithStringKey
{
    Task AddItem(ShoppingCartItem item);
    Task<ShoppingCartItem[]> GetItems();
    Task Clear();
}

public class ShoppingCartGrain : Grain, IShoppingCartGrain
{
    private readonly IPersistentState<ShoppingCartState> state;

    public ShoppingCartGrain([PersistentState("ShoppingCartState")]IPersistentState<ShoppingCartState> state)
    {
        this.state = state;
    }

    public async Task AddItem(ShoppingCartItem item)
    {
        var existingItem = state.State.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existingItem is null)
        {
            state.State.Items.Add(item);
        }
        else
        {
            existingItem.Count += item.Count;
        }
        await state.WriteStateAsync();
    }

    public Task<ShoppingCartItem[]> GetItems()
        => Task.FromResult(state.State.Items.ToArray());

    public Task Clear()
    {
        DeactivateOnIdle();
        return state.ClearStateAsync();
    }

    public class ShoppingCartState
    {
        public List<ShoppingCartItem> Items { get; set; } = new();
    }
}

[GenerateSerializer]
public class ShoppingCartItem
{
    [Id(0)]
    public int ProductId { get; set; }
    [Id(1)]
    public string ProductName { get; set; } = string.Empty;
    [Id(2)]
    public decimal Price { get; set; }
    [Id(3)]
    public int Count { get; set; }
}
