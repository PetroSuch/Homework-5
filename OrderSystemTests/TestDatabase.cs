using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderSystem.Data;

namespace OrderSystemTests;

internal static class TestDatabase
{
    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();

    public static DbContextOptions<ProductDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), DatabaseRoot)
            .Options;
    }
}
