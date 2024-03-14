public class LobbyDto
{
    public LobbyDto() 
    {
        Id = "";
        Players = new List<PlayerInfoDto>();
        // PlayedCards = new List<string>();
        RoundNumber = 0;
        // JudgeIndex = 0;
        LobbyHistory = new Dictionary<string, string>();
    }
    public Dictionary<string, string> LobbyHistory { get; set; }
    public List<string> CardHistory => LobbyHistory.SelectMany(e => new List<string> {e.Key, e.Value}).ToList();
    public string Id { get; set; }
    public List<PlayerInfoDto> Players { get; set; }
    // public List<string> PlayedCards { get; set; }
    public int RoundNumber { get; set; }
    // public int JudgeIndex { get; set; }

    public LobbyDto(Lobby lobby)
    {
        Id = lobby.Id;
        Players = lobby.Players.Select(player => new PlayerInfoDto(player)).ToList();
        // PlayedCards = lobby.PlayedCards;
        RoundNumber = lobby.RoundNumber;
        // JudgeIndex = lobby.JudgeIndex;
        LobbyHistory = lobby.LobbyHistory;
    }
}
