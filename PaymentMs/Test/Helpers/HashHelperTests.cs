using Core.Helpers;

namespace Test.Helpers;

public class HashHelperTests
{
    [Fact]
    public void ComputeSha256Hash_KnownValue()
    {
        var hash = HashHelper.ComputeSha256Hash("abc");
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
    }
}
