using LinkPara.Kkb.Application.Features.Kkb.Queries;
using NUnit.Framework;

namespace LinkPara.Kkb.IntegrationTests.KkbExternalServiceTests;
public class IbanValidationTests
{
    [Test]
    public static async Task IbanValidation()
    {
        var result = await Testing.SendAsync(new ValidateIbanQuery()
        {
            Iban = "TR560006200139600009090751",
            IdentityNo = "30273030930"
        });

        //Assert.IsNotNull(result);
    }
}
