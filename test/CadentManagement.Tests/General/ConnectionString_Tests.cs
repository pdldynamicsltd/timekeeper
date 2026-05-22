using Microsoft.Data.SqlClient;
using Shouldly;
using Xunit;

namespace CadentManagement.Tests.General;

// ReSharper disable once InconsistentNaming
public class ConnectionString_Tests
{
    [Fact]
    public void SqlConnectionStringBuilder_Test()
    {
        var csb = new SqlConnectionStringBuilder("Server=localhost; Database=CadentManagement; Trusted_Connection=True;");
        csb["Database"].ShouldBe("CadentManagement");
    }
}
