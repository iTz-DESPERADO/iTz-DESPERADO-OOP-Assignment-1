using OrderSystem.Domain;
using OrderSystem.ValueObject;
using OrderSystem.Pricing;

namespace OrderSystem.Helpers;

internal sealed class ConsoleApplication(OrderSystem system)
{
    public void Run()
    {
        while (true)
        {
            int choice = Display.ShowMenu("ORDER SYSTEM | Main menu", [
                "Customers",
                "Products",
                "Orders",
                "Reports",
                "Exit"], backAction: Display.IsInteractive ? "Exit confirmation" : "Exit program",
                describeAction: index => index == 4
                    ? (Display.IsInteractive ? "Exit confirmation" : "Exit program")
                    : $"Open {new[] { "Customers", "Products", "Orders", "Reports" }[index]}");
            if (choice is -1 or 4)
            {
                if (!Display.IsInteractive || Display.Confirm("Exit | Session data will be cleared", "Exit program"))
                {
                    Console.WriteLine("Goodbye. Session ended.");
                    return;
                }
                continue;
            }
            Execute(() =>
            {
                switch (choice)
                {
                    case 0: CustomersMenu(); break;
                    case 1: ProductsMenu(); break;
                    case 2: OrdersMenu(); break;
                    case 3: ReportsMenu(); break;
                }
            });
        }
    }

    private static void Execute(Action action)
    {
        try { action(); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OverflowException)
        {
            Display.Message("Action could not be completed", ex is OverflowException
                ? "The entered quantities or amounts are too large. Please use smaller values."
                : ex.Message);
        }
    }

    private static void Section(string title, (string Label, Action Action)[] actions)
    {
        while (true)
        {
            int choice = Display.ShowMenu(title, [.. actions.Select(action => action.Label), "Back to main menu"],
                backAction: "Main menu");
            if (choice < 0 || choice == actions.Length) return;
            Execute(actions[choice].Action);
        }
    }

    private void CustomersMenu() => Section("CUSTOMERS", [
        ("Add new customer", AddCustomer),
        ("View all customers", () => ShowCustomers(system.Customers)),
        ("Search by name, email or city", SearchCustomers),
        ("Edit customer details", EditCustomer),
        ("View a customer's orders", CustomerOrders)]);

    private void ProductsMenu() => Section("PRODUCTS & INVENTORY", [
        ("Add new product", AddProduct),
        ("View all products", () => ShowProducts(system.Products)),
        ("Restock a product", Restock),
        ("Change product price", ChangePrice),
        ("View low stock (5 or fewer)", () => ShowProducts(system.GetLowStock(), "LOW STOCK | 5 or fewer"))]);

    private void OrdersMenu() => Section("ORDERS", [
        ("Create new order", CreateOrder),
        ("View all orders / details", () => BrowseOrders(system.Orders, "ALL ORDERS")),
        ("View a customer's orders", CustomerOrders),
        ("View unpaid orders", () => BrowseOrders(system.GetUnpaidOrders(), "UNPAID ORDERS")),
        ("Record order payment", PayOrder),
        ("Discount settings", ChangeDiscounts),
        ("Add product to an unpaid order", AddProductToOrder)]);

    private void ReportsMenu() => Section("REPORTS", [
        ("Sales summary (all dates)", () => SalesSummary(system.Orders, "SALES SUMMARY | All dates")),
        ("Sales summary by date", DateReport),
        ("Customer order history", CustomerOrders),
        ("Inventory and low stock", InventoryReport),
        ("Best-selling products (paid orders)", BestSellers)]);

    private void AddCustomer()
    {
        const string title = "CUSTOMERS | Add new customer";
        var name = Display.ReadRequired(title, "1/4 - Full name:", submitAction: "Next: email", cancelAction: "Cancel new customer");
        if (name is null) return;
        var email = ReadEmail(title, "2/4 - Email address:");
        if (email is null) return;
        var city = Display.ReadRequired(title, "3/4 - City:", submitAction: "Next: customer type", cancelAction: "Cancel new customer");
        if (city is null) return;
        int type = Display.ShowMenu(title + " | 4/4 - Customer type", ["Regular", "VIP"],
            backAction: "Cancel new customer", describeAction: index => $"Create {(index == 0 ? "Regular" : "VIP")} customer");
        if (type < 0) return;
        var customer = system.AddCustomer(name, email, city, type == 0 ? CustomerType.Regular : CustomerType.Vip);
        Display.Message("Customer added", customer.ToString(), "Customers menu");
    }

    private string? ReadEmail(string title, string prompt, int? exceptId = null) => Display.ReadRequired(title, prompt, value =>
    {
        try { Email.Create(value); }
        catch (ArgumentException) { return "Enter a valid email address, for example name@example.com."; }
        return system.Customers.Any(customer => customer.Id != exceptId &&
            string.Equals(customer.Email.Value, value, StringComparison.OrdinalIgnoreCase))
            ? "This email is already registered to another customer." : null;
    }, submitAction: exceptId.HasValue ? "Review email change" : "Next: city",
        cancelAction: exceptId.HasValue ? "Return to customer fields" : "Cancel new customer");

    private void EditCustomer()
    {
        var customer = PickCustomer("CUSTOMERS | Choose customer to edit");
        if (customer is null) return;
        string title = $"EDIT CUSTOMER #{customer.Id}";
        string[] fields = ["name", "email", "city", "customer type"];
        while (true)
        {
            int field = Display.ShowMenu(title + " | Choose a field to change", [
                $"Name: {customer.Name}",
                $"Email: {customer.Email}",
                $"City: {customer.City}",
                $"Customer type: {customer.Type}",
                "Done - back to Customers"], backAction: "Customers menu",
                describeAction: index => index == 4 ? "Customers menu" : $"Change {fields[index]} only");
            if (field is -1 or 4) return;

            string name = customer.Name;
            string email = customer.Email.Value;
            string city = customer.City;
            CustomerType type = customer.Type;
            switch (field)
            {
                case 0:
                    var newName = Display.ReadRequired(title, $"Current name: {name}\nNew name:",
                        submitAction: "Review name change", cancelAction: "Return to customer fields");
                    if (newName is null) continue;
                    name = newName;
                    break;
                case 1:
                    var newEmail = ReadEmail(title, $"Current email: {email}\nNew email:", customer.Id);
                    if (newEmail is null) continue;
                    email = newEmail;
                    break;
                case 2:
                    var newCity = Display.ReadRequired(title, $"Current city: {city}\nNew city:",
                        submitAction: "Review city change", cancelAction: "Return to customer fields");
                    if (newCity is null) continue;
                    city = newCity;
                    break;
                case 3:
                    int selectedType = Display.ShowMenu(title + $" | Current type: {type}", ["Regular", "VIP"], customer.IsVip ? 1 : 0,
                        backAction: "Return to customer fields",
                        describeAction: index => $"Review change to {(index == 0 ? "Regular" : "VIP")}");
                    if (selectedType < 0) continue;
                    type = selectedType == 0 ? CustomerType.Regular : CustomerType.Vip;
                    break;
            }

            if (name == customer.Name && email == customer.Email.Value && city == customer.City && type == customer.Type)
            {
                Display.Message("No changes", "The selected value is unchanged.", "Return to customer fields");
                continue;
            }
            if (!Display.Confirm(title, $"Save {fields[field]} change")) continue;
            switch (field)
            {
                case 0:
                    system.UpdateCustomerName(customer.Id, name);
                    break;
                case 1:
                    system.UpdateCustomerEmail(customer.Id, email);
                    break;
                case 2:
                    system.UpdateCustomerCity(customer.Id, city);
                    break;
                case 3:
                    system.UpdateCustomerType(customer.Id, type);
                    break;
            }
            Display.Message("Customer updated", $"Updated {fields[field]}.\n{customer}", "Return to customer fields");
        }
    }

    private void SearchCustomers()
    {
        var query = Display.ReadRequired("SEARCH CUSTOMERS", "Enter part of a name, email or city:",
            submitAction: "Search customers", cancelAction: "Customers menu");
        if (query is not null) ShowCustomers(system.SearchCustomers(query));
    }

    private static void ShowCustomers(IReadOnlyList<Customer> customers) => Display.ShowScreen("CUSTOMERS", () =>
    {
        if (customers.Count == 0) { Console.WriteLine("No customers found. Use Customers > Add new customer to get started."); return; }
        foreach (var customer in customers)
        {
            Console.WriteLine($"#{customer.Id} | {customer.Name} | {customer.Type} | Email: {customer.Email} | City: {customer.City}");
        }
        Console.WriteLine($"\nCustomers: {customers.Count}");
    }, "Customers menu");

    private Customer? PickCustomer(string title) => Pick(title, system.Customers,
        customer => $"#{customer.Id} | {customer.Name} | {customer.Type}",
        "No customers yet. Add a customer in the Customers section first.", selectAction: "Select customer", backAction: "Cancel customer selection");

    private static T? Pick<T>(string title, IReadOnlyList<T> items, Func<T, string> label, string empty,
        string selectAction = "Select item", string backAction = "Cancel selection") where T : class
    {
        if (items.Count == 0) { Display.Message(title, empty, backAction); return null; }
        int choice = Display.ShowMenu(title, [.. items.Select(label), "Back"], backAction: backAction,
            describeAction: index => index == items.Count ? backAction : selectAction);
        return choice < 0 || choice == items.Count ? null : items[choice];
    }

    private void AddProduct()
    {
        const string title = "PRODUCTS | Add new product";
        var name = Display.ReadRequired(title, "1/3 - Product name:", submitAction: "Next: unit price", cancelAction: "Cancel new product");
        if (name is null) return;
        var price = Display.ReadDecimal(title, "2/3 - Unit price:", submitAction: "Next: opening stock", cancelAction: "Cancel new product");
        if (price is null) return;
        var stock = Display.ReadNumber(title, "3/3 - Opening stock (0 or more):", 0,
            submitAction: "Create product", cancelAction: "Cancel new product");
        if (stock is null) return;
        var product = system.AddProduct(name, price.Value, stock.Value);
        Display.Message("Product added", product.ToString(), "Products menu");
    }

    private Product? PickProduct(string title) => Pick(title, system.Products, product => product.ToString(),
        "No products yet. Use Products > Add new product first.", selectAction: "Select product", backAction: "Products menu");

    private void Restock()
    {
        var product = PickProduct("RESTOCK | Choose product");
        if (product is null) return;
        if (product.Stock == int.MaxValue) { Display.Message("Stock limit", "This product already has the maximum stock.", "Products menu"); return; }
        var quantity = Display.ReadNumber("RESTOCK", $"{product.Name} | Current stock: {product.Stock}\nUnits to add:", 1, int.MaxValue - product.Stock,
            submitAction: "Add units to stock", cancelAction: "Cancel restocking");
        if (quantity is null) return;
        system.RestockProduct(product.Id, quantity.Value);
        Display.Message("Stock updated", product.ToString(), "Products menu");
    }

    private void ChangePrice()
    {
        var product = PickProduct("PRICE | Choose product");
        if (product is null) return;
        var price = Display.ReadDecimal("CHANGE PRICE", $"{product.Name} | Current price: {product.Price:F2}\nNew unit price:",
            submitAction: "Review price confirmation", cancelAction: "Cancel price change");
        if (price is null || !Display.Confirm("CHANGE PRICE", $"Set price to {price:F2}")) return;
        system.ChangeProductPrice(product.Id, price.Value);
        Display.Message("Price updated", "New orders will use the new price.\n" + product, "Products menu");
    }

    private static void ShowProducts(IReadOnlyList<Product> products, string title = "PRODUCTS") => Display.ShowScreen(title, () =>
    {
        if (products.Count == 0) { Console.WriteLine("No products to display."); return; }
        foreach (var product in products)
            Console.WriteLine($"{product}  {(product.Stock == 0 ? "[OUT OF STOCK]" : product.Stock <= 5 ? "[LOW STOCK]" : "")}");
        Console.WriteLine($"\nProducts: {products.Count}");
    }, "Products menu");

    private void CreateOrder()
    {
        var customer = PickCustomer("NEW ORDER | 1 - Choose customer");
        if (customer is null) return;
        if (!system.Products.Any(product => product.Stock > 0))
        {
            Display.Message("No available stock", "Add or restock a product before creating an order.", "Orders menu");
            return;
        }
        var date = Display.ReadDate("NEW ORDER | 2 - Date", "Order date:", DateOnly.FromDateTime(DateTime.Today),
            submitAction: "Next: build order cart", cancelAction: "Cancel new order");
        if (date is null) return;
        var cart = new Dictionary<int, int>();
        while (true)
        {
            decimal subtotal = CartSubtotal(cart);
            decimal total = system.CalculateQuote(customer.Id, subtotal).Total;
            int action = Display.ShowMenu($"NEW ORDER | {customer.Name} | {cart.Count} product(s) | Total {total:F2}", [
                "Add product", "Review cart", "Remove product", "Confirm and create order", "Cancel order"],
                backAction: "Cancel order; discard cart",
                describeAction: index => new[] { "Choose a product", "Review cart", "Choose product to remove",
                    "Review order before confirmation", "Cancel order; discard cart" }[index]);
            if (action is -1 or 4) return;
            switch (action)
            {
                case 0:
                    var available = system.Products.Where(product => product.Stock > cart.GetValueOrDefault(product.Id)).ToList();
                    var product = Pick("NEW ORDER | Choose product", available,
                        p => $"#{p.Id} | {p.Name} | Price {p.Price:F2} | Available {p.Stock - cart.GetValueOrDefault(p.Id)}",
                        "No more stock is available. Review your cart or confirm the order.",
                        selectAction: "Choose quantity for this product", backAction: "Return to cart");
                    if (product is null) break;
                    int remaining = product.Stock - cart.GetValueOrDefault(product.Id);
                    var quantity = Display.ReadNumber("NEW ORDER | Quantity", $"{product.Name} | Enter quantity (1 - {remaining}):", 1, remaining,
                        submitAction: "Add quantity to cart", cancelAction: "Return to cart without adding");
                    if (quantity is null) break;
                    try
                    {
                        _ = checked(subtotal + product.Price * quantity.Value);
                        cart[product.Id] = checked(cart.GetValueOrDefault(product.Id) + quantity.Value);
                    }
                    catch (OverflowException) { Display.Message("Amount too large", "Use a smaller quantity or price.", "Return to cart"); }
                    break;
                case 1:
                    ShowCart(customer, date.Value, cart);
                    break;
                case 2:
                    var removed = Pick("REMOVE FROM CART", system.Products.Where(p => cart.ContainsKey(p.Id)).ToList(),
                        p => $"{p.Name} | Quantity {cart[p.Id]}", "The cart is empty.",
                        selectAction: "Remove this product from cart", backAction: "Return to cart without removing");
                    if (removed is not null) cart.Remove(removed.Id);
                    break;
                case 3:
                    if (cart.Count == 0) { Display.Message("Empty cart", "Add at least one product first.", "Return to cart"); break; }
                    ShowCart(customer, date.Value, cart, "Continue to order confirmation");
                    if (!Display.Confirm($"CREATE ORDER | Total {total:F2}", "Confirm order and reserve stock")) break;
                    var order = system.CreateOrder(customer.Id, date.Value,
                        cart.Select(item => new RequestedProduct(item.Key, item.Value)).ToList());
                    Display.ShowScreen($"Order #{order.Id} created successfully", () => PrintOrder(order), "Orders menu");
                    return;
            }
        }
    }

    private decimal CartSubtotal(Dictionary<int, int> cart) => system.Products
        .Where(product => cart.ContainsKey(product.Id)).Sum(product => product.Price * cart[product.Id]);

    private void ShowCart(Customer customer, DateOnly date, Dictionary<int, int> cart,
        string returnAction = "Return to cart") => Display.ShowScreen("ORDER REVIEW", () =>
    {
        Console.WriteLine($"Customer: {customer.Name} | {customer.Type} | Date: {date:yyyy-MM-dd}\n");
        if (cart.Count == 0) { Console.WriteLine("The cart is empty."); return; }
        foreach (var product in system.Products.Where(p => cart.ContainsKey(p.Id)))
            Console.WriteLine($"{product.Name} | {cart[product.Id]} x {product.Price:F2} = {product.Price * cart[product.Id]:F2}");
        decimal subtotal = CartSubtotal(cart);
        decimal total = system.CalculateQuote(customer.Id, subtotal).Total;
        Console.WriteLine($"\nSubtotal: {subtotal:F2}\nDiscount: {subtotal - total:F2}\nTotal due: {total:F2}");
        Console.WriteLine("Stock is reserved only after confirmation. Payment is recorded separately.");
    }, returnAction);

    private void CustomerOrders()
    {
        var customer = PickCustomer("CUSTOMER ORDER HISTORY | Choose customer");
        if (customer is null) return;
        var orders = system.GetCustomerOrders(customer.Id);
        Display.ShowScreen($"CUSTOMER SUMMARY | {customer.Name}", () =>
        {
            Console.WriteLine(customer);
            Console.WriteLine($"Orders: {orders.Count}");
            Console.WriteLine($"Paid: {orders.Where(o => o.IsPaid).Sum(o => system.CalculateOrderTotal(o.Id)):F2}");
            Console.WriteLine($"Outstanding: {orders.Where(o => !o.IsPaid).Sum(o => system.CalculateOrderTotal(o.Id)):F2}");
        }, "View customer orders");
        BrowseOrders(orders, $"ORDERS | {customer.Name}");
    }

    private string OrderLabel(Order order) => $"#{order.Id} | {order.Date:yyyy-MM-dd} | {order.Customer.Name} | {system.CalculateOrderTotal(order.Id):F2} | {(order.IsPaid ? "PAID" : "UNPAID")}";

    private void BrowseOrders(IReadOnlyList<Order> orders, string title)
    {
        if (orders.Count == 0) { Display.Message(title, "No orders found."); return; }
        while (true)
        {
            var order = Pick(title, orders.OrderByDescending(o => o.Date).ThenByDescending(o => o.Id).ToList(), OrderLabel, "No orders found.",
                selectAction: "View order details", backAction: "Return to previous menu");
            if (order is null) return;
            Display.ShowScreen($"ORDER #{order.Id}", () => PrintOrder(order), "Return to order list");
        }
    }

    private void PrintOrder(Order order)
    {
        var pricing = system.GetOrderPricing(order.Id);
        Console.WriteLine($"Order #{order.Id} | Date: {order.Date:yyyy-MM-dd} | {(order.IsPaid ? "PAID" : "UNPAID")}");
        Console.WriteLine($"Customer: #{order.Customer.Id} {order.Customer.Name}\nEmail: {order.Customer.Email}\n");
        foreach (var line in order.Lines)
            Console.WriteLine($"#{line.Product.Id} {line.Product.Name} | {line.Quantity} x {line.UnitPrice:F2} = {line.Total:F2}");
        Console.WriteLine($"\nSubtotal: {pricing.Subtotal:F2}\nDiscount ({pricing.DiscountRate:P2}): {pricing.DiscountAmount:F2}\nTotal: {pricing.Total:F2}");
        Console.WriteLine($"Outstanding: {(order.IsPaid ? 0m : pricing.Total):F2}");
    }

    private void AddProductToOrder()
    {
        var order = Pick("ADD PRODUCT | Choose unpaid order", system.GetUnpaidOrders(), OrderLabel,
            "There are no unpaid orders.", selectAction: "Choose order", backAction: "Orders menu");
        if (order is null) return;

        var product = Pick("ADD PRODUCT | Choose product", system.Products.Where(p => p.Stock > 0).ToList(),
            p => p.ToString(), "No products are in stock. Add or restock a product first.",
            selectAction: "Choose quantity", backAction: "Cancel adding product");
        if (product is null) return;

        var quantity = Display.ReadNumber("ADD PRODUCT | Quantity",
            $"{product.Name} | Available stock: {product.Stock}\nQuantity:", 1, product.Stock,
            submitAction: "Review addition", cancelAction: "Cancel adding product");
        if (quantity is null) return;

        var existingLine = order.Lines.FirstOrDefault(line => line.Product.Id == product.Id);
        decimal unitPrice = existingLine?.UnitPrice ?? product.Price;
        decimal subtotal = checked(order.Total + unitPrice * quantity.Value);
        decimal total = system.CalculateQuote(order.Customer.Id, subtotal).Total;
        if (!Display.Confirm($"ORDER #{order.Id} | New total {total:F2}",
            $"Add {quantity.Value} x {product.Name} and reserve stock")) return;

        system.AddLineToOrder(order.Id, product.Id, quantity.Value);
        Display.ShowScreen($"Order #{order.Id} updated", () => PrintOrder(order), "Orders menu");
    }

    private void PayOrder()
    {
        var order = Pick("PAYMENT | Choose unpaid order", system.GetUnpaidOrders(), OrderLabel, "There are no unpaid orders.",
            selectAction: "Review order before payment", backAction: "Orders menu");
        if (order is null) return;
        Display.ShowScreen("PAYMENT | Review order", () => PrintOrder(order), "Continue to payment confirmation");
        if (!Display.Confirm($"PAYMENT | Order #{order.Id} | {system.CalculateOrderTotal(order.Id):F2}", "Confirm payment received")) return;
        system.MarkOrderPaid(order.Id);
        Display.Message("Payment recorded", $"Order #{order.Id} is now PAID. Amount: {system.CalculateOrderTotal(order.Id):F2}", "Orders menu");
    }

    private void SalesSummary(IReadOnlyList<Order> orders, string title) => Display.ShowScreen(title, () =>
    {
        var prices = orders.Select(order => (Order: order, Pricing: system.GetOrderPricing(order.Id))).ToList();
        Console.WriteLine($"Orders: {orders.Count}");
        Console.WriteLine($"Paid orders: {orders.Count(o => o.IsPaid)} | Unpaid orders: {orders.Count(o => !o.IsPaid)}");
        Console.WriteLine($"Order value (after discounts): {prices.Sum(item => item.Pricing.Total):F2}");
        Console.WriteLine($"Paid sales: {prices.Where(item => item.Order.IsPaid).Sum(item => item.Pricing.Total):F2}");
        Console.WriteLine($"Outstanding: {prices.Where(item => !item.Order.IsPaid).Sum(item => item.Pricing.Total):F2}");
        Console.WriteLine($"Discounts: {prices.Sum(item => item.Pricing.DiscountAmount):F2}");
        Console.WriteLine("\nPaid sales include only orders marked as paid.");
        Console.WriteLine("Date filters use the order date, not the payment date.");
    }, "Reports menu");

    private void ChangeDiscounts()
    {
        const string title = "DISCOUNT SETTINGS";
        var policy = system.DiscountPolicy;
        string current = $"Regular: {policy.RegularRate:P2} | VIP: {policy.VipRate:P2}";
        Display.Message(title, current + "\nChanges apply to new and unpaid orders. Paid amounts stay unchanged.", "Continue to discount entry");
        var regular = Display.ReadPercentage(title, "Regular customer discount (%):", submitAction: "Next: VIP discount");
        if (regular is null) return;
        var vip = Display.ReadPercentage(title, "VIP customer discount (%):", submitAction: "Review discount confirmation");
        if (vip is null) return;
        if (!Display.Confirm(title, $"Apply Regular {regular:F2}% / VIP {vip:F2}%")) return;
        system.SetDiscountPolicy(new CustomerDiscountPolicy(regular.Value / 100m, vip.Value / 100m));
        Display.Message("Discounts updated", "New and unpaid orders now use the updated discounts.", "Orders menu");
    }

    private void DateReport()
    {
        var from = Display.ReadDate("SALES BY DATE", "Start date:", submitAction: "Next: end date", cancelAction: "Reports menu");
        if (from is null) return;
        DateOnly? to;
        while (true)
        {
            to = Display.ReadDate("SALES BY DATE", "End date:", DateOnly.FromDateTime(DateTime.Today),
                submitAction: "Show sales report", cancelAction: "Reports menu");
            if (to is null) return;
            if (to >= from) break;
            Display.Message("Check date range", "End date must be on or after the start date.", "Re-enter end date");
        }
        SalesSummary(system.GetOrdersByDate(from.Value, to.Value), $"SALES | {from:yyyy-MM-dd} to {to:yyyy-MM-dd} (inclusive)");
    }

    private void InventoryReport() => Display.ShowScreen("INVENTORY REPORT", () =>
    {
        Console.WriteLine($"Products: {system.Products.Count}");
        Console.WriteLine($"Units in stock: {system.Products.Sum(p => (long)p.Stock)}");
        Console.WriteLine($"Stock value (current selling prices): {system.Products.Sum(p => p.Price * p.Stock):F2}");
        Console.WriteLine($"Out of stock: {system.Products.Count(p => p.Stock == 0)}");
        Console.WriteLine("\nLow stock (5 units or fewer):");
        var lowStock = system.GetLowStock();
        if (lowStock.Count == 0) Console.WriteLine("No low-stock products.");
        foreach (var product in lowStock) Console.WriteLine(product);
    }, "Reports menu");

    private void BestSellers() => Display.ShowScreen("BEST-SELLING PRODUCTS | Paid orders only", () =>
    {
        var products = system.Orders.Where(order => order.IsPaid).SelectMany(order => order.Lines)
            .GroupBy(line => line.Product.Id)
            .Select(group => new { Id = group.Key, Name = group.First().Product.Name,
                Units = group.Sum(line => (long)line.Quantity), Gross = group.Sum(line => line.Total) })
            .OrderByDescending(product => product.Units).ThenBy(product => product.Id).ToList();
        if (products.Count == 0) { Console.WriteLine("No paid orders yet."); return; }
        foreach (var product in products)
            Console.WriteLine($"#{product.Id} {product.Name} | Units: {product.Units} | Gross sales: {product.Gross:F2}");
        Console.WriteLine("\nGross sales are before customer discounts.");
    }, "Reports menu");

}
