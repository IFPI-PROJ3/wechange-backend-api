using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using Proj3.Application.Common.Interfaces.Services.Authentication.Command;
using Proj3.Application.Common.Interfaces.Services.Authentication.Queries;
using Proj3.Application.Services.Authentication.Result;
using Proj3.Contracts.Authentication.Request;
using Proj3.Contracts.Authentication.Response;
using System.Net.Mime;

namespace Proj3.Api.Controllers.Authentication
{
    /// <summary>
    /// Authentication controller
    /// </summary>
    [ApiController]    
    [Route("auth")]
    [ApiVersion("1.0")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationCommandService _authenticationCommandService;
        private readonly IAuthenticationQueryService _authenticationQueryService;
        //private readonly IEmailCommandService _emailCommandService;

        /// <summary>
        /// Authentication controller constructor
        /// </summary>
        public AuthenticationController(IAuthenticationCommandService authenticationCommandService, IAuthenticationQueryService authenticationQueryService /*, IEmailCommandService emailCommandService */)
        {
            _authenticationCommandService = authenticationCommandService;
            _authenticationQueryService = authenticationQueryService;
            //_emailCommandService = emailCommandService;
        }

        /// <summary>
        /// Ngo user signup
        /// </summary>
        /// <param name="request">User data</param>
        /// <seealso cref="SignUpNgoRequest"/>
        /// <response code="201">User created</response>
        /// <response code="409">User already exists</response>
        /// <response code="422">User validation error</response> 
        /// <response code="500">Internal server error</response>
        [ProducesResponseType(typeof(UserStatusResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        [HttpPost("signup-ngo")]
        public async Task<IActionResult> SignUpNgo([FromBody] SignUpNgoRequest request)
        {
            UserStatusResult? userInactiveResult = await _authenticationCommandService.SignUpNgoAsync(
                request
            );

            UserStatusResponse? response = new
            (
                userInactiveResult.user.Id,
                userInactiveResult.user.UserName,
                userInactiveResult.user.Email,
                userInactiveResult.user.Active
            );

            return StatusCode(StatusCodes.Status200OK, response);
        }

        /// <summary>
        /// Volunteer user signup
        /// </summary>
        /// <param name="SignUpVolunteerRequest">User info</param>
        /// <response code="201">User created</response>
        /// <response code="409">User already exists</response>
        /// <response code="422">User validation error</response>        
        /// <response code="500">InternalServerError</response>  
        [ProducesResponseType(typeof(UserStatusResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        [HttpPost("signup-volunteer")]
        public async Task<IActionResult> SignUpVolunteer([FromBody] SignUpVolunteerRequest request)
        {
            UserStatusResult? userInactiveResult = await _authenticationCommandService.SignUpVolunteerAsync(
                request
            );

            UserStatusResponse? response = new
            (
                userInactiveResult.user.Id,
                userInactiveResult.user.UserName,
                userInactiveResult.user.Email,
                userInactiveResult.user.Active
            );

            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>
        /// User signin
        /// </summary>
        /// <param name="request">User data</param>
        /// <response code="200">User authenticated</response>
        /// <response code="401">Unauthorized</response>   
        /// <response code="500">InternalServerError</response>  
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            AuthenticationResult? authServiceResult = await _authenticationQueryService.SignInAsync(
                request
            );

            AuthenticationResponse? response = new
            (
                id: authServiceResult.user.Id,
                username: authServiceResult.user.UserName,
                email: authServiceResult.user.Email,
                active: authServiceResult.user.Active,
                role: authServiceResult.user.UserRole.GetDisplayName(),
                access_token: authServiceResult.AccessToken,
                authServiceResult.RefreshToken
            );

            return StatusCode(StatusCodes.Status200OK, response);
        }

        /// <summary>
        /// Logout user
        /// </summary>        
        /// <response code="204">No content, refresh</response>
        /// <response code="401">Unauthorized</response>                   
        /// <response code="500">InternalServerError</response>  
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]        
        [HttpPut("logout")]
        public async Task<IActionResult> Logout()
        {
            if(await _authenticationCommandService.LogoutAsync(HttpContext) == false)
            {
                return Ok(StatusCodes.Status401Unauthorized);
            }
            
            return Ok(StatusCodes.Status204NoContent);
        }

        /// <summary>
        /// Receive new refresh token        
        /// </summary>        
        /// <param name="request">Access token and refresh token</param>        
        /// <response code="200">User authenticated</response>
        /// <response code="401">Invalid tokens</response>                           
        /// <response code="500">InternalServerError</response>  
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]        
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        [HttpPut("refresh-token")]
        public async Task<ActionResult> RefreshTokenAsync([FromBody]RefreshTokenRequest request)
        {            
            AuthenticationResult? authResult = await _authenticationCommandService.RefreshTokenAsync(
                request
            );

            AuthenticationResponse? response = new
            (      
                authResult.user.Id,
                authResult.user.UserName,
                authResult.user.Email,
                authResult.user.Active,
                authResult.user.UserRole.GetDisplayName(),
                authResult.AccessToken,
                authResult.RefreshToken
            );

            return StatusCode(StatusCodes.Status200OK, response);
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">User credentials</param>
        /// <response code="201">User authenticated</response>
        /// <response code="401">Invalid credentials exception</response>
        /// <response code="401">Invalid password exception</response>
        /// <response code="500">InternalServerError</response>
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            AuthenticationResult? authServiceResult = await _authenticationCommandService.ChangePasswordAsync(
                request
            );

            AuthenticationResponse? response = new
            (
                id: authServiceResult.user.Id,
                username: authServiceResult.user.UserName,
                email: authServiceResult.user.Email,
                active: authServiceResult.user.Active,
                role: nameof(authServiceResult.user.UserRole),
                access_token: authServiceResult.AccessToken,
                refresh_token: authServiceResult.RefreshToken
            );

            return StatusCode(StatusCodes.Status200OK, response);
        }
    }
}

