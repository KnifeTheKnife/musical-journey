namespace musical_journey.Services.Interfaces;

public interface IDbCreate
{
    /// <summary>
    /// Creates the database if it does not exist.
    /// </summary>
    /// <returns>int status: 0 -> operation sucessful; -1 -> operation failed (db probably already exists.</returns>
    int DbCreate();
}
