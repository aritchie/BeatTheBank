using BeatTheBank.Models;
using SQLite;

namespace BeatTheBank.Services;


public class AppSqliteConnection : SQLiteAsyncConnection
{
    public AppSqliteConnection() : base(Path.Combine(FileSystem.AppDataDirectory, "btb.db"), true)
    {
        var conn = this.GetConnection();
        conn.CreateTable<Game>();
        conn.CreateTable<GameVault>();
        conn.CreateTable<Player>();
    }
}