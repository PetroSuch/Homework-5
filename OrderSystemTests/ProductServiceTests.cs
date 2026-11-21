using NUnit.Framework;
using OrderSystem.Data;
using OrderSystem.Services;

namespace OrderSystemTests;

public class ProductServiceTests
{
    private ProductDbContext _db = null!;
    private ProductService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new ProductDbContext(TestDatabase.CreateOptions());
        _db.Database.EnsureCreated();
        _service = new ProductService(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public void SearchProductsByName_MatchesPartOfNameIgnoringCase()
    {
        var product = _service.SearchProductsByName("TOP").Single();

        Assert.That((product.Name, product.Category?.Name), Is.EqualTo(("Laptop", "Electronics")));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void SearchProductsByName_ReturnsAllProductsForBlankSearch(string? search)
    {
        var names = _service.SearchProductsByName(search).Select(product => product.Name);

        Assert.That(names, Is.EquivalentTo(new[] { "Laptop", "Smartphone", "Novel" }));
    }

    [Test]
    public void SearchProductsByName_ReturnsEmptyListWhenNothingMatches()
    {
        Assert.That(_service.SearchProductsByName("Camera"), Is.Empty);
    }

    [Test]
    public void GetProductById_ReturnsProductWithCategory()
    {
        var product = _service.GetProductById(3);

        Assert.That((product?.Name, product?.Category?.Name), Is.EqualTo(("Novel", "Books")));
    }

    [Test]
    public void GetProductById_ReturnsNullWhenProductDoesNotExist()
    {
        Assert.That(_service.GetProductById(999), Is.Null);
    }

    [Test]
    public void GetProductsByCategory_ReturnsOnlyProductsInThatCategory()
    {
        var names = _service.GetProductsByCategory(1).Select(product => product.Name);

        Assert.That(names, Is.EquivalentTo(new[] { "Laptop", "Smartphone" }));
    }

    [TestCase("Name", new[] { "Laptop", "Novel", "Smartphone" })]
    [TestCase("Price", new[] { "Novel", "Smartphone", "Laptop" })]
    public void GetProductsSorted_ReturnsProductsInOrder(string sortField, string[] expectedNames)
    {
        var names = _service.GetProductsSorted(sortField).Select(product => product.Name);

        Assert.That(names, Is.EqualTo(expectedNames));
    }

    [Test]
    public void SearchProducts_ReturnsProductsWithMatchingName()
    {
        var product = _service.SearchProducts("phone").Single();

        Assert.That(product.Name, Is.EqualTo("Smartphone"));
    }
}
