using Shouldly;
using System.Linq;
using Xunit;

namespace CadentManagement.Tests.General
{
    public class GetRandomString_Tests: AppTestBase
    {
        [Fact]
        public void GetRandomString_Should_Support_Uppercase_Regex()
        {
            var randomString = GetRandomString(50, 0, @"^[A-Z0-9\-]+$");
            randomString.Where(char.IsLetter).All(char.IsUpper).ShouldBeTrue();
        }
    }
}
