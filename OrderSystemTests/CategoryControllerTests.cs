using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OrderSystem.Controllers;
using OrderSystem.Data;
using OrderSystem.Models;

namespace OrderSystemTests;

public class CategoryControllerTests
{
    private ProductDbContext _db = null!;
    private CategoryController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new ProductDbContext(TestDatabase.CreateOptions());
        _db.Database.EnsureCreated();
        _controller = new CategoryController(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [TestCase(null, new[] { "Books", "Clothing", "Electronics" })]
    [TestCase("name_desc", new[] { "Electronics", "Clothing", "Books" })]
    public void Index_SortsCategoriesByName(string? sortOrder, string[] expectedNames)
    {
        var view = (ViewResult)_controller.Index(sortOrder);
        var categories = (List<CategoryListItem>)view.Model!;
        var names = new List<string>();

        foreach (var category in categories)
        {
            names.Add(category.Name);
        }

        Assert.That(names, Is.EqualTo(expectedNames));
    }

    [TestCase("Books", 1)]
    [TestCase("Clothing", 0)]
    [TestCase("Electronics", 2)]
    public void Index_ShowsProductCount(string categoryName, int expectedCount)
    {
        var view = (ViewResult)_controller.Index(null);
        var categories = (List<CategoryListItem>)view.Model!;
        var category = categories.Single(item => item.Name == categoryName);

        Assert.That(category.ProductCount, Is.EqualTo(expectedCount));
    }

    [TestCase(null, "name_desc")]
    [TestCase("name_desc", "name_asc")]
    public void Index_SetsNextSortOrder(string? sortOrder, string nextSortOrder)
    {
        var view = (ViewResult)_controller.Index(sortOrder);

        Assert.That(view.ViewData["NameSortParam"], Is.EqualTo(nextSortOrder));
    }
}
