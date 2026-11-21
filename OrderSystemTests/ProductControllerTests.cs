using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OrderSystem.Controllers;
using OrderSystem.Data;
using OrderSystem.Models;
using OrderSystem.Services;

namespace OrderSystemTests;

public class ProductControllerTests
{
    private ProductDbContext _db = null!;
    private Mock<IProductService> _productService = null!;
    private ProductController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new ProductDbContext(TestDatabase.CreateOptions());
        _db.Database.EnsureCreated();
        _productService = new Mock<IProductService>(MockBehavior.Strict);
        _controller = new ProductController(_db, _productService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [TestCase(null)]
    [TestCase("camera")]
    public void Index_ReturnsProductsFromService(string? search)
    {
        var products = new[] { new Product { Name = "Camera" } };
        _productService.Setup(service => service.SearchProductsByName(search)).Returns(products);

        var view = (ViewResult)_controller.Index(search);

        Assert.That(view.Model, Is.EqualTo(products));
    }

    [Test]
    public void Index_KeepsSearchText()
    {
        _productService.Setup(service => service.SearchProductsByName("camera"))
            .Returns(Array.Empty<Product>());

        var view = (ViewResult)_controller.Index("camera");

        Assert.That(view.ViewData["SearchString"], Is.EqualTo("camera"));
    }

    [Test]
    public void Create_Get_ShowsCategoryOptions()
    {
        var view = (ViewResult)_controller.Create();
        var categories = (SelectList)view.ViewData["Categories"]!;
        var options = categories.Select(category => (category.Value, category.Text));

        Assert.That(options, Is.EquivalentTo(new[]
        {
            ("1", "Electronics"), ("2", "Books"), ("3", "Clothing")
        }));
    }

    [Test]
    public void Create_Post_SavesValidProduct()
    {
        var product = new Product { Name = "Camera", Price = 120m, CategoryId = 1 };

        _controller.Create(product);

        var savedProduct = _db.Products.AsNoTracking().Single(item => item.Name == "Camera");
        Assert.That((savedProduct.Price, savedProduct.CategoryId), Is.EqualTo((120m, 1)));
    }

    [Test]
    public void Create_Post_RedirectsToIndex()
    {
        var product = new Product { Name = "Camera", Price = 120m, CategoryId = 1 };

        var result = (RedirectToActionResult)_controller.Create(product);

        Assert.That(result.ActionName, Is.EqualTo(nameof(ProductController.Index)));
    }

    [Test]
    public void Create_Post_ReturnsInvalidProduct()
    {
        var product = new Product { Price = 120m, CategoryId = 1 };
        _controller.ModelState.AddModelError(nameof(Product.Name), "Name is required.");

        var view = (ViewResult)_controller.Create(product);

        Assert.That(view.Model, Is.SameAs(product));
    }

    [Test]
    public void Create_Post_ShowsCategoriesForInvalidProduct()
    {
        _controller.ModelState.AddModelError(nameof(Product.Name), "Name is required.");

        var view = (ViewResult)_controller.Create(new Product { Price = 120m, CategoryId = 1 });

        Assert.That(view.ViewData["Categories"], Is.TypeOf<SelectList>());
    }

    [Test]
    public void Create_Post_DoesNotSaveInvalidProduct()
    {
        _controller.ModelState.AddModelError(nameof(Product.Name), "Name is required.");

        _controller.Create(new Product { Price = 120m, CategoryId = 1 });

        Assert.That(_db.Products.Count(), Is.EqualTo(3));
    }

    [Test]
    public void Details_ReturnsRequestedProduct()
    {
        var product = new Product { Id = 10, Name = "Camera" };
        _productService.Setup(service => service.GetProductById(10)).Returns(product);

        var view = (ViewResult)_controller.Details(10);

        Assert.That(view.Model, Is.SameAs(product));
    }

    [Test]
    public void Details_ReturnsNotFoundWhenProductDoesNotExist()
    {
        _productService.Setup(service => service.GetProductById(999)).Returns((Product?)null);

        var result = _controller.Details(999);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }
}
