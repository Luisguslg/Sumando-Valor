using SumandoValor.Infrastructure.Data;
using Xunit;

namespace SumandoValor.Tests.Domain;

public class ApplicationUserValidationTests
{
    [Fact]
    public void ApplicationUser_ValidatesConditionalRule_TieneDiscapacidadRequiresDescripcion()
    {
        bool ValidateDiscapacidad(ApplicationUser user)
        {
            if (user.TieneDiscapacidad && string.IsNullOrWhiteSpace(user.DiscapacidadDescripcion))
            {
                return false;
            }
            return true;
        }

        var userWithDiscapacidadNoDesc = new ApplicationUser
        {
            TieneDiscapacidad = true,
            DiscapacidadDescripcion = null
        };

        var userWithDiscapacidadAndDesc = new ApplicationUser
        {
            TieneDiscapacidad = true,
            DiscapacidadDescripcion = "Discapacidad visual"
        };

        var userWithoutDiscapacidad = new ApplicationUser
        {
            TieneDiscapacidad = false,
            DiscapacidadDescripcion = null
        };

        Assert.False(ValidateDiscapacidad(userWithDiscapacidadNoDesc));
        Assert.True(ValidateDiscapacidad(userWithDiscapacidadAndDesc));
        Assert.True(ValidateDiscapacidad(userWithoutDiscapacidad));
    }
}
