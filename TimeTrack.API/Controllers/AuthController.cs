using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTrack.API.Models.Responses;
using TimeTrack.Application.Features.Users.Commands.ForgotPassword;
using TimeTrack.Application.Features.Users.Commands.LoginUser;
using TimeTrack.Application.Features.Users.Commands.RegisterUser;
using TimeTrack.Application.Features.Users.Commands.ResendAuthOtp;
using TimeTrack.Application.Features.Users.Commands.ResetPassword;
using TimeTrack.Application.Features.Users.Commands.VerifyRegistrationOtp;

namespace TimeTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);

            var response = ApiResponse<RegisterUserResult>.Ok(
                result,
                "User and employee created successfully.");

            return Ok(response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);

            var response = ApiResponse<LoginOtpResult>.Ok(
                result,
                "OTP generated successfully.");

            return Ok(response);
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyRegistrationOtpCommand command)
        {
            var result = await _mediator.Send(command);

            var response = ApiResponse<LoginResult>.Ok(
                result,
                "OTP verified. Login successful.");

            return Ok(response);
        }

        [HttpPost("resend-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtp([FromBody] ResendAuthOtpCommand command)
        {
            var result = await _mediator.Send(command);

            var response = ApiResponse<LoginOtpResult>.Ok(
                result,
                "OTP resent successfully.");

            return Ok(response);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var result = await _mediator.Send(command);

            var response = ApiResponse<ForgotPasswordResult>.Ok(
                result,
                "OTP generated for password reset.");

            return Ok(response);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var result = await _mediator.Send(command);

            var response = ApiResponse<ResetPasswordResult>.Ok(
                result,
                "Password reset successful. Please login with your new password.");

            return Ok(response);
        }
    }
}


//Clean Archi
//Layers 
//CQRS+MediatR
//Global Error Handling
//Fluent Validation
//Dependency Injection
//Identity
//JWT
//Register => OTP => Verify OTP => Login
//Login => OTP => Verify OTP => Get JWT => login
//Resend otp
//ForgotPasswordRequest 
//Reset Password
//Authentication Manager
//serilog
//Email with service bus
//SSE bell

//pdfgenerator
//Azure Functions
//SMS
//Redis Caching

