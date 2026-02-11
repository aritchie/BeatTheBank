using System.Runtime.CompilerServices;
using SQLite;

namespace BeatTheBank.Services;


public class GameDatabase
{
    readonly SQLiteAsyncConnection connection;

    public GameDatabase(string dbPath)
    {
        this.connection = new SQLiteAsyncConnection(dbPath);
        this.connection.GetConnection().CreateTable<GameResult>();
    }


    public Task<int> SaveGameResultAsync(GameResult result)
        => this.connection.InsertAsync(result);


    public Task<List<GameResult>> GetPlayerGamesAsync(string playerName)
        => this.connection
            .Table<GameResult>()
            .Where(g => g.PlayerName == playerName)
            .OrderByDescending(g => g.CompletedAt)
            .ToListAsync();

    public Task<List<GameResult>> GetAllGamesAsync()
        => this.connection
            .Table<GameResult>()
            .OrderByDescending(g => g.CompletedAt)
            .ToListAsync();
}


/// <summary>
/// Simple async lazy initialization helper.
/// </summary>
public class AsyncLazy<T>
{
    readonly Lazy<Task<T>> inner;

    public AsyncLazy(Func<Task<T>> factory)
    {
        this.inner = new Lazy<Task<T>>(factory);
    }

    public TaskAwaiter<T> GetAwaiter() => this.inner.Value.GetAwaiter();
}
