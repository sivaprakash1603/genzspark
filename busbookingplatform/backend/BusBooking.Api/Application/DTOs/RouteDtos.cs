namespace BusBooking.Api.Application.DTOs;

public record CreateSourceRequest(string Name);
public record CreateDestinationRequest(string Name);
public record CreateRouteRequest(Guid SourceId, Guid DestinationId);
public record RouteResponse(Guid Id, string Source, string Destination);
