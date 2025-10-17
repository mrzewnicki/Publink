using MediatR;
using Microsoft.AspNetCore.Mvc;
using Publink.Core.CQRS.Queries;
using Publink.Shared.Dtos;

namespace Publink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets paginated audit logs with optional sorting.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="sort">Sorting format: "Property:asc|desc" (e.g., "CreatedDate:desc").</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<GetLogsQuery.Response>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 20;

        var request = new GetLogsQuery.Request(new QueryDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Sort = sort
        });

        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}