using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infraestructura.Bodega;

public class FabricaContextoBodega : IDesignTimeDbContextFactory<ContextoBodega>
{
    public ContextoBodega CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContextoBodega>();
        optionsBuilder.UseSqlite("Data Source=bodega.db");
        return new ContextoBodega(optionsBuilder.Options);
    }
}
