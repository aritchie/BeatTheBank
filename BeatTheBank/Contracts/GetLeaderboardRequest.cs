namespace BeatTheBank.Contracts;


public record GetLeaderboardRequest(int TopN = 10) : IRequest<List<PlayerStats>>;
