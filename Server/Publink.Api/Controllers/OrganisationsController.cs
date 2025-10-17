using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Publink.Core.CQRS.Queries;

namespace Publink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganisationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrganisationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns all organisations (ids and names) aggregated from document headers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetOrganisationsQuery.Response), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<GetOrganisationsQuery.Response>> Get(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetOrganisationsQuery.Request(), cancellationToken);
        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => BadRequest(response),
            _ => Ok(response)
        };
    }
}
