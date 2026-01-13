using Microsoft.Data.Sqlite;
using musical_journey.Services.Interfaces;

namespace musical_journey.Services;

public class DbC : IDbCreate{

    public int DbCreate(){
        string connectionString = "Data Source=cache.db;Version=3;";
        string query = "CREATE TABLE IF NOT EXISTS Songs (Path Text PRIMARY KEY, Title TEXT, Artist TEXT, Album TEXT, TrackNo INT, Date Date, Genre TEXT, DiscNo INT);";
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var command = new SqliteCommand(query, connection);
        if (command.ExecuteNonQuery()!=0){
            return 0;
        } else{
            return -1;
        }
    }
}
