using OrderSystem.Domain;
using OrderSystem.Repositories;
using OrderSystem.Extensions;
using OrderSystem.Pricing;

namespace OrderSystem;

internal class OrderSystem
{
    private readonly CustomerRepository _customerRepository = new();
    private readonly ProductRepository _productRepository = new();
    private readonly OrderRepository _orderRepository = new();
    private readonly Dictionary<int, OrderPricing> _paidPricing = [];

    public CustomerDiscountPolicy DiscountPolicy { get; private set; }

    public OrderSystem(CustomerDiscountPolicy? discountPolicy = null)
    {
        DiscountPolicy = discountPolicy ?? new CustomerDiscountPolicy();
    }

    public void SetDiscountPolicy(CustomerDiscountPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        DiscountPolicy = policy;
    }

    public OrderPricing CalculateQuote(int customerId, decimal subtotal)
        => OrderExtensions.CalculatePricing(subtotal, _customerRepository.GetById(customerId), DiscountPolicy);

    public OrderPricing GetOrderPricing(int orderId)
    {
        var order = _orderRepository.GetById(orderId);
        return _paidPricing.TryGetValue(orderId, out var paidPricing)
            ? paidPricing : order.CalculatePricing(DiscountPolicy);
    }

    public IReadOnlyList<Customer> Customers => _customerRepository.GetAll();
    public IReadOnlyList<Product> Products => _productRepository.GetAll();
    public IReadOnlyList<Order> Orders => _orderRepository.GetAll();

    public Customer AddCustomer(string name, string email, string city, CustomerType type)
        => _customerRepository.Add(name, email, city, type);

    public void UpdateCustomerName(int id, string name)
        => _customerRepository.UpdateName(id, name);

    public void UpdateCustomerEmail(int id, string email)
        => _customerRepository.UpdateEmail(id, email);

    public void UpdateCustomerCity(int id, string city)
        => _customerRepository.UpdateCity(id, city);

    public void UpdateCustomerType(int id, CustomerType type)
        => _customerRepository.UpdateCustomerType(id, type);

    public IReadOnlyList<Customer> SearchCustomers(string query) => _customerRepository.Search(query);
    public Product AddProduct(string name, decimal price, int stock) => _productRepository.Add(name, price, stock);
    public void RestockProduct(int id, int quantity) => _productRepository.GetById(id).IncreaseStock(quantity);
    public void ChangeProductPrice(int id, decimal price) => _productRepository.GetById(id).ChangePrice(price);
    public IReadOnlyList<Product> GetLowStock(int threshold = 5) => _productRepository.GetLowStock(threshold);

    public IReadOnlyList<Order> GetCustomerOrders(int customerId)
    {
        _customerRepository.GetById(customerId);
        return _orderRepository.GetByCustomerId(customerId);
    }

    public IReadOnlyList<Order> GetUnpaidOrders() => _orderRepository.GetUnpaid();
    public IReadOnlyList<Order> GetOrdersByDate(DateOnly from, DateOnly to)
        => _orderRepository.GetByDateRange(from, to);

    public Order CreateOrder(int customerId, DateOnly date, List<RequestedProduct> requestedProducts)
    {
        ArgumentNullException.ThrowIfNull(requestedProducts);
        if (requestedProducts.Count == 0)
            throw new InvalidOperationException("Order must contain at least one product.");

        var customer = _customerRepository.GetById(customerId);
        foreach (var item in requestedProducts)
        {
            ArgumentNullException.ThrowIfNull(item);
            if (item.ProductId <= 0 || item.Quantity <= 0)
                throw new ArgumentException("Product ID and quantity must be greater than zero.");
        }

        var items = requestedProducts.GroupBy(item => item.ProductId)
            .Select(group => new RequestedProduct(group.Key, group.Sum(item => item.Quantity))).ToArray();

        var products = _productRepository.GetByIds(items.Select(item => item.ProductId))
            .ToDictionary(product => product.Id);

        decimal subtotal = 0;
        foreach (var item in items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
                throw new InvalidOperationException($"Product #{item.ProductId} was not found.");

            if (product.Stock < item.Quantity)
                throw new InvalidOperationException($"Not enough stock for {product.Name}. Available: {product.Stock}; requested: {item.Quantity}.");

            subtotal = subtotal + product.Price * item.Quantity;
        }

        _ = CalculateQuote(customerId, subtotal);
        var order = _orderRepository.Add(customer, date);

        foreach (var item in items)
        {
            var product = products[item.ProductId];

            order.AddLine(product, item.Quantity);
        }

        return order;
    }

    public void AddLineToOrder(int orderId, int productId, int quantity)
    {
        var order = _orderRepository.GetById(orderId);
        var product = _productRepository.GetById(productId);
        order.AddLine(product, quantity);
    }

    public void MarkOrderPaid(int orderId)
    {
        var order = _orderRepository.GetById(orderId);

        if (_paidPricing.ContainsKey(orderId)) return;

        var pricing = order.CalculatePricing(DiscountPolicy);

        order.MarkPaid();

        _paidPricing.Add(orderId, pricing);
    }

    public decimal CalculateOrderTotal(int orderId) => GetOrderPricing(orderId).Total;
    public decimal TotalSalesPaidOnly() => Orders.Where(order => order.IsPaid)
        .Sum(order => CalculateOrderTotal(order.Id));
}

internal sealed record RequestedProduct(int ProductId, int Quantity);
