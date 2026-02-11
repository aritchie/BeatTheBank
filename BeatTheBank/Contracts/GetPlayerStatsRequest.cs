namespace BeatTheBank.Contracts;


public record GetPlayerStatsRequest(string PlayerName) : IRequest<PlayerStats?>;
