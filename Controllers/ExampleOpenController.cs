using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneLoginSampleAuthApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneLoginSampleAuthApi.Controllers
{
    /// <summary>
    /// second example controller
    /// with open endpoint
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public class ExampleOpenController: ControllerBase
    {
        /// <summary>
        /// returns standard pong message
        /// </summary>
        /// <returns>200 - {"message": "pong"}</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PongResult), StatusCodes.Status200OK)]
        public PongResult Ping()
        {
            return new PongResult() { Message = "pong" };
        }
    }
}
