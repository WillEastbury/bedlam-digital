public class MyLobbyDto
{
    public MyLobbyDto() 
    {
        Id = "";
        Players = new List<PlayerInfoDto>();
        PlayedCards = new List<string>();
        RoundNumber = 0;
        JudgeIndex = 0;
    }
    public string Id { get; set; }
    public List<PlayerInfoDto> Players { get; set; }
    public List<string> PlayedCards { get; set; }
    public int RoundNumber { get; set; }
    public int JudgeIndex { get; set; }
    // I want to flatten this into a sequential list of cards 
    public Dictionary<string, string> LobbyHistory { get; set; } = new(); 
    public List<string> CardHistory => LobbyHistory.SelectMany(e => new List<string> {e.Key, e.Value}).ToList();
    public MyLobbyDto(Lobby lobby)
    {
        Id = lobby.Id;
        Players = lobby.Players.Select(player => new PlayerInfoDto(player)).ToList();
        PlayedCards = lobby.PlayedCards;
        RoundNumber = lobby.RoundNumber;
        JudgeIndex = lobby.JudgeIndex;
        LobbyHistory = lobby.LobbyHistory;
    }
    
}
