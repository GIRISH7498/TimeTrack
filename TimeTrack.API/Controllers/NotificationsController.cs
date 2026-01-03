using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TimeTrack.API.Models.Responses;
using TimeTrack.API.Services.Notifications;
using TimeTrack.Application.Common.Interfaces;

namespace TimeTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ISseConnectionManager _connectionManager;
        private readonly ICurrentUserService _currentUserService;

        public NotificationsController(
            IMediator mediator,
            ISseConnectionManager connectionManager,
            ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _connectionManager = connectionManager;
            _currentUserService = currentUserService;
        }

        // =======================
        // Bell notifications REST
        // =======================

        // GET /api/notifications/bell
        //[HttpGet("bell")]
        //public async Task<IActionResult> GetBellInbox([FromQuery] int? take)
        //{
        //    var result = await _mediator.Send(new GetMyBellInboxQuery(take));

        //    var response = ApiResponse<IReadOnlyList<BellNotificationDto>>.Ok(
        //        result,
        //        "Bell notifications fetched successfully.");

        //    return Ok(response);
        //}

        //// POST /api/notifications/bell/{inboxId}/read
        //[HttpPost("bell/{inboxId:long}/read")]
        //public async Task<IActionResult> MarkBellRead(long inboxId)
        //{
        //    await _mediator.Send(new MarkBellNotificationReadCommand(inboxId));

        //    var response = ApiResponse<object>.Ok(null, "Bell notification marked as read.");
        //    return Ok(response);
        //}

        //// ==========================
        //// SSE stream (real-time bell)
        //// ==========================

        //// GET /api/notifications/stream
        //[HttpGet("stream")]
        //public async Task Stream(CancellationToken cancellationToken)
        //{
        //    if (_currentUserService.UserId is null)
        //    {
        //        Response.StatusCode = StatusCodes.Status401Unauthorized;
        //        return;
        //    }

        //    var userId = _currentUserService.UserId.Value;

        //    Response.Headers.Add("Content-Type", "text/event-stream");
        //    Response.Headers.Add("Cache-Control", "no-cache");
        //    Response.Headers.Add("X-Accel-Buffering", "no"); // for proxies like Nginx
        //    Response.StatusCode = StatusCodes.Status200OK;

        //    // register SSE client
        //    var client = _connectionManager.AddClient(userId);

        //    try
        //    {
        //        // Optional initial event
        //        await WriteEventAsync(Response, "{\"message\":\"connected\"}", cancellationToken);

        //        // read messages from the channel and send as SSE
        //        await foreach (var message in client.MessageChannel.Reader.ReadAllAsync(cancellationToken))
        //        {
        //            await WriteEventAsync(Response, message, cancellationToken);
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        // client disconnected / request aborted
        //    }
        //    finally
        //    {
        //        _connectionManager.RemoveClient(userId, client);
        //    }
        //}

        //private static async Task WriteEventAsync(
        //    HttpResponse response,
        //    string json,
        //    CancellationToken cancellationToken)
        //{
        //    var sb = new StringBuilder();
        //    sb.Append("data: ");
        //    sb.Append(json.Replace("\n", "\\n")); // basic safety for newlines in JSON
        //    sb.Append("\n\n");

        //    await response.WriteAsync(sb.ToString(), cancellationToken);
        //    await response.Body.FlushAsync(cancellationToken);
        //}
    }
}
