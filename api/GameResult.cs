public class GameResult
{
    public string LobbyName { get; set; } = "";
    public List<PlayerScoreEntry> Players { get; set; } = new();
    public DateTime Date { get; set; } = DateTime.UtcNow;
}

public class PlayerScoreEntry
{
    public string Name { get; set; } = "";
    public int Score { get; set; }
}
