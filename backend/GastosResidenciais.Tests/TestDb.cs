using GastosResidenciais.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Tests;

internal static class TestDb
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
