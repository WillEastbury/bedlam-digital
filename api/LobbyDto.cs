public class LobbyDto
{
    public LobbyDto() 
    {
        Id = "";
        Players = new List<PlayerInfoDto>();
        // PlayedCards = new List<string>();
        // RoundNumber = 0;
        // JudgeIndex = 0;
    }
    public string Id { get; set; }
    public List<PlayerInfoDto> Players { get; set; }
    // public List<string> PlayedCards { get; set; }
    // public int RoundNumber { get; set; }
    // public int JudgeIndex { get; set; }

    public LobbyDto(Lobby lobby)
    {
        Id = lobby.Id;
        Players = lobby.Players.Select(player => new PlayerInfoDto(player)).ToList();
        // PlayedCards = lobby.PlayedCards;
        // RoundNumber = lobby.RoundNumber;
        // JudgeIndex = lobby.JudgeIndex;
    }
}
