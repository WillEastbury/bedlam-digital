using Xunit;

[Collection("CardTests")]
public class DtoTests
{

    [Fact]
    public void PlayerInfoDto_DefaultConstructor()
    {
        var dto = new PlayerInfoDto();
        Assert.Equal("", dto.Name);
        Assert.Equal(0, dto.Score);
        Assert.Equal("", dto.LastPlayedCard);
    }

    [Fact]
    public void PlayerInfoDto_FromPlayer()
    {
        var player = new Player("Alice");
        player.WonRound();
        player.LastPlayedCard = "Card1";

        var dto = new PlayerInfoDto(player);

        Assert.Equal("Alice", dto.Name);
        Assert.Equal(1, dto.Score);
        Assert.Equal("Card1", dto.LastPlayedCard);
    }

    [Fact]
    public void CardsDto_DefaultValues()
    {
        var dto = new CardsDto();
        Assert.Equal("", dto.QuestionCard);
        Assert.NotNull(dto.AnswerCards);
        Assert.Empty(dto.AnswerCards);
    }

    [Fact]
    public void LobbyDto_DefaultConstructor()
    {
        var dto = new LobbyDto();
        Assert.Equal("", dto.Id);
        Assert.NotNull(dto.Players);
        Assert.Empty(dto.Players);
        Assert.Equal(0, dto.RoundNumber);
        Assert.NotNull(dto.LobbyHistory);
    }

    [Fact]
    public void LobbyDto_FromLobby()
    {
        var lobby = new Lobby("TestLobby");
        lobby.AddPlayer(new Player("P1"));
        lobby.AddPlayer(new Player("P2"));

        var dto = new LobbyDto(lobby);

        Assert.Equal("TestLobby", dto.Id);
        Assert.Equal(2, dto.Players.Count);
        Assert.Equal(1, dto.RoundNumber);
        Assert.Equal("P1", dto.Players[0].Name);
    }

    [Fact]
    public void LobbyDto_CardHistory_FlattensPairs()
    {
        var dto = new LobbyDto();
        dto.LobbyHistory.Add("Q1", "A1");
        dto.LobbyHistory.Add("Q2", "A2");

        var history = dto.CardHistory;
        Assert.Equal(4, history.Count);
        Assert.Contains("Q1", history);
        Assert.Contains("A1", history);
        Assert.Contains("Q2", history);
        Assert.Contains("A2", history);
    }

    [Fact]
    public void MyLobbyDto_DefaultConstructor()
    {
        var dto = new MyLobbyDto();
        Assert.Equal("", dto.Id);
        Assert.NotNull(dto.Players);
        Assert.Empty(dto.PlayedCards);
        Assert.Equal(0, dto.RoundNumber);
        Assert.Equal(0, dto.JudgeIndex);
    }

    [Fact]
    public void MyLobbyDto_FromLobby()
    {
        var lobby = new Lobby("TestLobby");
        var player = new Player("P1");
        player.Cards.Add("Card1");
        lobby.AddPlayer(player);

        var dto = new MyLobbyDto(lobby);

        Assert.Equal("TestLobby", dto.Id);
        Assert.Single(dto.Players);
        Assert.Equal(1, dto.RoundNumber);
        Assert.Equal(0, dto.JudgeIndex);
    }

    [Fact]
    public void MyLobbyDto_CardHistory_FlattensPairs()
    {
        var dto = new MyLobbyDto();
        dto.LobbyHistory.Add("Q1", "A1");

        var history = dto.CardHistory;
        Assert.Equal(2, history.Count);
        Assert.Equal("Q1", history[0]);
        Assert.Equal("A1", history[1]);
    }

    [Fact]
    public void MyLobbyDto_IncludesPlayedCards()
    {
        var lobby = new Lobby("TestLobby");
        lobby.AddPlayer(new Player("Judge"));
        var p = new Player("Player");
        p.Cards.Add("CardX");
        lobby.AddPlayer(p);

        lobby.PlayCardForPlayer(p.Id, "CardX");

        var dto = new MyLobbyDto(lobby);
        Assert.Single(dto.PlayedCards);
        Assert.Contains("CardX", dto.PlayedCards);
    }
}
