#nullable enable
using LinkPara.Location.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Location.Infrastructure;

public class DataSeeder
{
    private readonly ApplicationDbContext _dbContext;

    public DataSeeder(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task Initialize(ApplicationDbContext dbContext)
    {
        await Seed(dbContext);
    }

    private async Task Seed(ApplicationDbContext dbContext)
    {
    }
   
}

