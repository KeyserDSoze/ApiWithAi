using Microsoft.AspNetCore.Components;

namespace ApiWithAi.AiControllers
{
    public abstract class AiBaseController
    {
    }
    [Route("[controller]/[action]")]
    public class CountryAiController : AiBaseController
    {
        public async Task SomethingAsync()
        {

        }
    }
}
