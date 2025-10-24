using System.Threading;
using System.Threading.Tasks;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Abstractions;

/// <summary>
/// Defines how collaborative coding session state is stored and retrieved across Redis and EF Core.
/// </summary>
public interface IClubSessionCoordinator
{
    Task<ClubSessionStateDto> GetSessionAsync(string sessionId, CancellationToken cancellationToken);

    Task<ClubSessionStateDto> ApplyUpdateAsync(ClubSessionUpdateDto update, CancellationToken cancellationToken);
}
