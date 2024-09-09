import React, { MouseEvent } from "react";
import './App.css';

function App() {
  const handleClick = async (event: MouseEvent) => {
    event.preventDefault();
    
    const response = await fetch("/api/shopping-cart");
    console.log(await response.json());
  };
  
  const handleProductClick = async (event: MouseEvent) => {
    event.preventDefault();
    
    const response = await fetch("/api/products/1");
    console.log(await response.json());
  };
  
  const handleFeaturedProductsClick = async (event: MouseEvent) => {
    event.preventDefault();
    
    const response = await fetch("/api/products/featured");
    console.log(await response.json());
  };
  
  const handleAddShoppingCartClick = async (event: MouseEvent) => {
    event.preventDefault();
    
    const response = await fetch('/api/shopping-cart', {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({productId: 1, count: 1})
    });
    console.log(await response.text());
  };
  
  const handleOrderClick = async (event: MouseEvent) => {
    event.preventDefault();
    
    var address = {
      name: 'Chris Klug',
      street1: 'The Street 1',
      postalCode: '666',
      city: 'Whoville',
      country: 'Ladonia'
    };
    const response = await fetch('/api/orders', {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        deliveryAddress: address,
        billingAddress: address,
        items: [
          {
            itemId: 1,
            quantity: 2
          },
          {
            itemId: 3,
            quantity: 1
          },
          {
            itemId: 5,
            quantity: 4
          }
        ]
      })
    });
    console.log(await response.json());
  };

  const handleLoginClick = async (event: MouseEvent) => {
    event.preventDefault();
    document.location.href = "/auth/login"
  };

  return (
    <div className="App">
      <header className="App-header">
        <button onClick={handleClick}>Load Shopping Cart</button>
        <button onClick={handleAddShoppingCartClick}>Add Product to Shopping Cart</button>
        <button onClick={handleProductClick}>Get Product 1</button>
        <button onClick={handleFeaturedProductsClick}>Get Featured Products</button>
        <button onClick={handleOrderClick}>Add Order</button>
        <button onClick={handleLoginClick}>Log In</button>
      </header>
    </div>
  );
}

export default App;
