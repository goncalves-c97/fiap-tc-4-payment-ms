using Core.Helpers;

namespace Test.Helpers;

public class ErrorsTests
{
    private enum MyErrors
    {
        One = 1,
        Two = 2
    }

    [Fact]
    public void RegisterError_WithoutProperties_AddsErrorAndSummaryContainsEnumAndMessage()
    {
        var errors = new Errors();
        errors.RegisterError(MyErrors.One, "msg");

        Assert.Single(errors);
        Assert.Contains("One", errors.Summary);
        Assert.Contains("msg", errors.Summary);
    }

    [Fact]
    public void RegisterError_WithProperties_StoresPropertiesNamesSummary()
    {
        var errors = new Errors();
        errors.RegisterError(MyErrors.Two, "msg", "A", "B");

        var error = Assert.Single(errors);
        Assert.Equal("A, B", error.PropertiesNamesSummary);
    }
}
