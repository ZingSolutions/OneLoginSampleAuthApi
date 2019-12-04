using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneLoginSampleAuthApi.Models;

namespace OneLoginSampleAuthApi.Controllers
{
    /// <summary>
    /// own base controller so can setup default attributes
    /// required by majority of controllers
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public abstract class ApiControllerBase : ControllerBase
    {
    }
}
