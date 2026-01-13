using SumandoValor.Infrastructure.Services;
using Xunit;

namespace SumandoValor.Tests.Services;

public class CaptchaValidatorTests
{
    [Fact]
    public async Task MockCaptchaValidator_AlwaysReturnsTrue()
    {
        var validator = new MockCaptchaValidator();
        var result = await validator.ValidateAsync("test-token", "127.0.0.1");
        
        Assert.True(result);
    }

    [Fact]
    public async Task MockCaptchaValidator_ReturnsTrue_EvenWithEmptyToken()
    {
        var validator = new MockCaptchaValidator();
        var result = await validator.ValidateAsync("", null);
        
        Assert.True(result);
    }
}
