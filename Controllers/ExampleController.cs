using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneLoginSampleAuthApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OneLoginSampleAuthApi.Controllers
{
    /// <summary>
    /// example API with endpoints for testing
    /// </summary>
    public class ExampleController: ApiControllerBase
    {
        /// <summary>
        /// returns pong and the authenticated callers name.
        /// </summary>
        /// <returns>200 - {"message": "pong [username]"}</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PongResult), StatusCodes.Status200OK)]
        public PongResult ProtectedPing()
        {
            return new PongResult() { Message = $"pong {User.Identity.Name}" };
        }

        /// <summary>
        /// outputs the claims for the authenticated caller
        /// </summary>
        /// <returns>200 - array of claims (key value pairs)</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<KeyValuePair<string, string>>), StatusCodes.Status200OK)]
        public List<KeyValuePair<string, string>> ShowClaims()
        {
            return User.Claims.Select(e => new KeyValuePair<string, string>(e.Type, e.Value)).ToList();
        }

        /// <summary>
        /// Example endpoint to test the global and local error handlers
        /// </summary>
        /// <param name="message">custom message. if set to "OK" will be echoed back with a 200 response. 
        /// if missing will return a 401 error, otherwise an exception will be thrown</param>
        /// <returns>500 error if message provided. 400 if no message set.</returns>
        /// <response code="200">message was OK</response>
        /// <response code="400">missing message query string parameter</response>
        /// <response code="500">message was not OK</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckMessageResponse),StatusCodes.Status200OK)]
        public IActionResult CheckMessage([FromQuery]string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return new BadRequestObjectResult(new ApiErrorResponse() { StatusCode = 400, Message = "Message was missing"});
            }
            if(message.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
            {
                return new OkObjectResult(new CheckMessageResponse() { Message = message });
            }
            throw new ApplicationException(string.IsNullOrWhiteSpace(message) ? "message supplied was not OK" : message);
        }
    }
}
