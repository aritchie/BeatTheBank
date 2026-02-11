// using BeatTheBank.Models;
// using BeatTheBank.Contracts;
// using Microsoft.Extensions.Logging;
// using Shiny.Mediator;
// using Shiny.Mediator.Infrastructure;
//
// namespace BeatTheBank.Tests.ViewModels;
//
// public class LeaderboardViewModelTests
// {
//     readonly IMediator mediator;
//     readonly LeaderboardViewModel vm;
//
//     public LeaderboardViewModelTests()
//     {
//         mediator = Substitute.For<IMediator>();
//         var logger = Substitute.For<ILogger<LeaderboardViewModel>>();
//         vm = new LeaderboardViewModel(mediator, logger);
//     }
//
//     [Fact]
//     public void InitialState_PlayersIsEmpty()
//     {
//         vm.Players.ShouldBeEmpty();
//         vm.IsRefreshing.ShouldBeFalse();
//     }
//
//     [Fact]
//     public async Task RefreshCommand_PopulatesPlayers()
//     {
//         var stats = new List<PlayerStats>
//         {
//             new() { PlayerName = "Alice", TotalWon = 5000, GamesPlayed = 10 },
//             new() { PlayerName = "Bob", TotalWon = 3000, GamesPlayed = 5 },
//         };
//
//         var wrapper = Substitute.For<IRequestResultWrapper<List<PlayerStats>>>();
//         wrapper.Result.Returns(stats);
//
//         mediator.Request(Arg.Any<GetLeaderboardRequest>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
//             .Returns(wrapper);
//
//         await vm.RefreshCommand.ExecuteAsync(null);
//
//         vm.Players.Count.ShouldBe(2);
//         vm.Players[0].PlayerName.ShouldBe("Alice");
//         vm.Players[1].PlayerName.ShouldBe("Bob");
//         vm.IsRefreshing.ShouldBeFalse();
//     }
//
//     [Fact]
//     public async Task RefreshCommand_RequestsTop20()
//     {
//         var wrapper = Substitute.For<IRequestResultWrapper<List<PlayerStats>>>();
//         wrapper.Result.Returns(new List<PlayerStats>());
//
//         mediator.Request(Arg.Any<GetLeaderboardRequest>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
//             .Returns(wrapper);
//
//         await vm.RefreshCommand.ExecuteAsync(null);
//
//         await mediator.Received(1).Request(
//             Arg.Is<GetLeaderboardRequest>(r => r.TopN == 20),
//             Arg.Any<CancellationToken>(),
//             Arg.Any<Action<IMediatorContext>>()
//         );
//     }
//
//     [Fact]
//     public async Task RefreshCommand_SetsIsRefreshingFalseAfterCompletion()
//     {
//         var wrapper = Substitute.For<IRequestResultWrapper<List<PlayerStats>>>();
//         wrapper.Result.Returns(new List<PlayerStats>());
//
//         mediator.Request(Arg.Any<GetLeaderboardRequest>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
//             .Returns(wrapper);
//
//         await vm.RefreshCommand.ExecuteAsync(null);
//
//         vm.IsRefreshing.ShouldBeFalse();
//     }
// }
