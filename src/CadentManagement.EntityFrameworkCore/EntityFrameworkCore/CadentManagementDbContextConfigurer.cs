using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace CadentManagement.EntityFrameworkCore;

public static class CadentManagementDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<CadentManagementDbContext> builder, string connectionString)
    {
        builder.UseSqlServer(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<CadentManagementDbContext> builder, DbConnection connection)
    {
        builder.UseSqlServer(connection);
    }
}

