using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OrderSystem.Controllers;
using OrderSystem.Data;
using OrderSystem.Models;

namespace OrderSystemTests;

public class OrderControllerTests
{
    private DbContextOptions<ProductDbContext> _options = null!;
    private Mock<ProductDbContext> _dbMock = null!;
    private OrderController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _options = TestDatabase.CreateOptions();
        _dbMock = new Mock<ProductDbContext>(_options) { CallBase = true };
        _dbMock.Object.Database.EnsureCreated();
        _controller = new OrderController(_dbMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbMock.Object.Dispose();
    }

    [Test]
    public void Index_ReturnsSavedOrdersWithItemsInDescendingOrder()
    {
        using var seedDb = new ProductDbContext(_options);
        seedDb.Orders.AddRange(
            new Order { Id = 1, Items = { new OrderItem { ProductId = 1, Quantity = 2 } } },
            new Order { Id = 2, Items = { new OrderItem { ProductId = 3, Quantity = 4 } } });
        seedDb.SaveChanges();

        var view = (ViewResult)_controller.Index();
        var orders = (IReadOnlyList<Order>)view.Model!;

        Assert.That(orders.Select(order => order.Id), Is.EqualTo(new[] { 2, 1 }));
        Assert.That(orders[0].Items.Single().Quantity, Is.EqualTo(4));
        Assert.That(orders[1].Items.Single().Quantity, Is.EqualTo(2));
    }

    [Test]
    public void Index_ReturnsEmptyListWhenThereAreNoOrders()
    {
        var view = (ViewResult)_controller.Index();

        Assert.That(view.Model, Is.Empty);
    }

    [Test]
    public void Create_Get_ShowsProductsInDropdown()
    {
        var view = (ViewResult)_controller.Create();
        var products = (SelectList)view.ViewData["Products"]!;

        Assert.That(view.Model, Is.Null);
        Assert.That(products.Select(product => (product.Value, product.Text)), Is.EquivalentTo(new[]
        {
            ("1", "Laptop"), ("2", "Smartphone"), ("3", "Novel")
        }));
    }

    [Test]
    public void Create_Post_SavesValidOrderAndRedirectsToProducts()
    {
        var order = new Order { Items = { new OrderItem { ProductId = 1, Quantity = 2 } } };

        var result = (RedirectToActionResult)_controller.Create(order);

        using var verificationDb = new ProductDbContext(_options);
        var savedOrder = verificationDb.Orders.Include(item => item.Items).Single();
        var savedItem = savedOrder.Items.Single();
        Assert.That(result.ActionName, Is.EqualTo(nameof(ProductController.Index)));
        Assert.That(result.ControllerName, Is.EqualTo("Product"));
        Assert.That(savedItem.ProductId, Is.EqualTo(1));
        Assert.That(savedItem.Quantity, Is.EqualTo(2));
        _dbMock.Verify(db => db.SaveChanges(), Times.Once());
    }

    [Test]
    public void Create_Post_ReturnsInvalidOrderAndProductsWithoutSaving()
    {
        var order = new Order();
        _controller.ModelState.AddModelError("Items[0].Quantity", "Invalid quantity.");

        var view = (ViewResult)_controller.Create(order);

        Assert.That(view.Model, Is.SameAs(order));
        Assert.That(view.ViewData["Products"], Is.TypeOf<SelectList>());
        Assert.That(_dbMock.Object.Orders.Any(), Is.False);
        _dbMock.Verify(db => db.SaveChanges(), Times.Never());
    }
}
