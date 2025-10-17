using System.Net;
using MediatR;
using Publink.Data.Repositories;
using Publink.Shared.Dtos;

namespace Publink.Core.CQRS.Queries;

public static class GetOrganisationsQuery
{
    public sealed record Request() : IRequest<Response>;

    public sealed record Response(IEnumerable<OrganisationDto> Items, HttpStatusCode StatusCode = HttpStatusCode.OK);

    public sealed class Handler(IOrganisationRepository repository) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var items = await repository.GetNamesAllAsync(cancellationToken);
            var dtos = items.Select(i => new OrganisationDto
            {
                Id = i.Id,
                Name = i.Name ?? string.Empty
            });
            return new Response(dtos);
        }
    }
}
