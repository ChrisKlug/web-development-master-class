using WebDevMasterClass.Services.Orders.gRPC;
using WebDevMasterClass.Services.Orders.Tests.Infrastructure;

namespace WebDevMasterClass.Services.Orders.Tests;

public class OrderServiceTests
{
    [Fact]
    public Task Adds_Order_to_database()
        => TestHelper.ExecuteTest(async client =>
            {
                var request = new AddOrderRequest
                {
                    DeliveryAddress = new Address
                    {
                        Name = "Chris Klug",
                        Street1 = "Test street 1",
                        Street2 = "",
                        PostalCode = "12345",
                        City = "Stockholm",
                        Country = "Sweden"
                    },
                    BillingAddress = new Address
                    {
                        Name = "John Doe",
                        Street1 = "Some street 1",
                        Street2 = "",
                        PostalCode = "56789",
                        City = "Whoville",
                        Country = "Denmark"
                    }
                };

                request.Items.Add(new OrderItem
                {
                    Name = "My Product",
                    Price = 123.5f,
                    Quantity = 2
                });

                var response = await client.AddOrderAsync(request);

                Assert.NotNull(response);
                Assert.True(response.Success);
                Assert.NotNull(response.OrderId);

            }, 
            validateDb: async cmd =>
            {
                int id;
                string orderId;
                cmd.CommandText = "SELECT * FROM Orders";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    Assert.True(reader.Read());
                    id = (int)reader["Id"];
                    orderId = (string)reader["OrderId"];
                    Assert.Equal(247, (decimal)reader["Total"]);
                    Assert.False(reader.Read());
                }
                cmd.CommandText = "SELECT * FROM OrderItems WHERE OrderId = " + id;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    Assert.True(reader.Read());
                    Assert.Equal("My Product", (string)reader["Name"]);
                    Assert.Equal(123.5m, (decimal)reader["Price"]);
                    Assert.Equal(2, (int)reader["Quantity"]);
                    Assert.False(reader.Read());
                }
                cmd.CommandText = "SELECT * FROM Addresses WHERE AddressType = 'Delivery' AND OrderId = " + id;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    Assert.True(reader.Read());
                    Assert.Equal("Chris Klug", (string)reader["Name"]);
                    Assert.Equal("Test street 1", (string)reader["Street1"]);
                    Assert.Equal("", (string)reader["Street2"]);
                    Assert.Equal("12345", (string)reader["PostalCode"]);
                    Assert.Equal("Stockholm", (string)reader["City"]);
                    Assert.Equal("Sweden", (string)reader["Country"]);
                    Assert.False(reader.Read());
                }
                cmd.CommandText = "SELECT * FROM Addresses WHERE AddressType = 'Billing' AND OrderId = " + id;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    Assert.True(reader.Read());
                    Assert.Equal("John Doe", (string)reader["Name"]);
                    Assert.Equal("Some street 1", (string)reader["Street1"]);
                    Assert.Equal("", (string)reader["Street2"]);
                    Assert.Equal("56789", (string)reader["PostalCode"]);
                    Assert.Equal("Whoville", (string)reader["City"]);
                    Assert.Equal("Denmark", (string)reader["Country"]);
                    Assert.False(reader.Read());
                }
            });
}