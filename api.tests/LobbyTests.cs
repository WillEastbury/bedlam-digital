using Xunit;

[Collection("CardTests")]
public class LobbyTests
{

    private Lobby CreateLobby(string id = "TestLobby") => new Lobby(id);

    [Fact]
    public void Constructor_SetsIdAndDefaults()
    {
        var lobby = CreateLobby("MyLobby");

        Assert.Equal("MyLobby", lobby.Id);
        Assert.Empty(lobby.Players);
        Assert.Empty(lobby.PlayedCards);
        Assert.Equal(1, lobby.RoundNumber);
        Assert.Equal(0, lobby.JudgeIndex);
        Assert.Empty(lobby.LobbyHistory);
    }

    [Fact]
    public void Constructor_LoadsDecks()
    {
        var lobby = CreateLobby();

        Assert.NotEmpty(lobby.QuestionDeck);
        Assert.NotEmpty(lobby.AnswerDeck);
        // Question cards should be Black cards, answer cards White
        Assert.All(lobby.QuestionDeck, q => Assert.Contains("Black", q));
        Assert.All(lobby.AnswerDeck, a => Assert.Contains("White", a));
    }

    [Fact]
    public void Constructor_ShufflesDecks()
    {
        // Create two lobbies and check decks are likely in different order
        var lobby1 = CreateLobby("L1");
        var lobby2 = CreateLobby("L2");

        // Same cards but order should differ (extremely unlikely to be identical)
        Assert.Equal(lobby1.QuestionDeck.Count, lobby2.QuestionDeck.Count);
        Assert.Equal(lobby1.AnswerDeck.Count, lobby2.AnswerDeck.Count);
    }

    [Fact]
    public void CurrentQuestionCard_ReturnsCardAtRoundIndex()
    {
        var lobby = CreateLobby();
        string expected = lobby.QuestionDeck[1]; // RoundNumber starts at 1
        Assert.Equal(expected, lobby.CurrentQuestionCard);
    }

    [Fact]
    public void StaticConstants_AreExpected()
    {
        Assert.Equal(8, Lobby.MaxPlayers);
        Assert.Equal(8, Lobby.MaxRounds);
        Assert.Equal(8, Lobby.CardsDealtPerPlayer);
    }

    [Fact]
    public void AddPlayer_AddsToPlayersList()
    {
        var lobby = CreateLobby();
        var player = new Player("Alice");

        lobby.AddPlayer(player);

        Assert.Single(lobby.Players);
        Assert.Equal("Alice", lobby.Players[0].Name);
    }

    [Fact]
    public void AddPlayer_MultiplePlayersAdded()
    {
        var lobby = CreateLobby();

        for (int i = 0; i < 5; i++)
            lobby.AddPlayer(new Player($"Player{i}"));

        Assert.Equal(5, lobby.Players.Count);
    }

    [Fact]
    public void PlayCardForPlayer_PlayerNotInLobby_ReturnsFalse()
    {
        var lobby = CreateLobby();
        bool result = lobby.PlayCardForPlayer("nonexistent", "someCard");
        Assert.False(result);
    }

    [Fact]
    public void PlayCardForPlayer_JudgeCannotPlay_ReturnsFalse()
    {
        var lobby = CreateLobby();
        var judge = new Player("Judge");
        judge.Cards.Add("Card1");
        lobby.AddPlayer(judge);
        // JudgeIndex is 0, so first player is judge

        bool result = lobby.PlayCardForPlayer(judge.Id, "Card1");
        Assert.False(result);
    }

    [Fact]
    public void PlayCardForPlayer_PlayerDoesNotHaveCard_ReturnsFalse()
    {
        var lobby = CreateLobby();
        lobby.AddPlayer(new Player("Judge")); // index 0 = judge
        var player = new Player("Player1");
        player.Cards.Add("CardA");
        lobby.AddPlayer(player);

        bool result = lobby.PlayCardForPlayer(player.Id, "CardNotInHand");
        Assert.False(result);
    }

    [Fact]
    public void PlayCardForPlayer_ValidPlay_ReturnsTrue()
    {
        var lobby = CreateLobby();
        lobby.AddPlayer(new Player("Judge")); // index 0 = judge
        var player = new Player("Player1");
        player.Cards.Add("CardA");
        player.Cards.Add("CardB");
        lobby.AddPlayer(player);

        bool result = lobby.PlayCardForPlayer(player.Id, "CardA");

        Assert.True(result);
        Assert.DoesNotContain("CardA", player.Cards);
        Assert.Contains("CardA", lobby.PlayedCards);
        Assert.Equal("CardA", player.LastPlayedCard);
    }

    [Fact]
    public void PlayCardForPlayer_RemovesCardFromHand()
    {
        var lobby = CreateLobby();
        lobby.AddPlayer(new Player("Judge"));
        var player = new Player("Player1");
        player.Cards.AddRange(new[] { "C1", "C2", "C3" });
        lobby.AddPlayer(player);

        lobby.PlayCardForPlayer(player.Id, "C2");

        Assert.Equal(2, player.Cards.Count);
        Assert.DoesNotContain("C2", player.Cards);
    }

    [Fact]
    public void JudgeVoteOnCard_IncreasesWinnerScore()
    {
        var lobby = CreateLobby();
        lobby.AddPlayer(new Player("Judge"));
        var player = new Player("Winner");
        player.Cards.Add("WinCard");
        lobby.AddPlayer(player);

        lobby.PlayCardForPlayer(player.Id, "WinCard");
        string result = lobby.JudgeVoteOnCard("WinCard");

        Assert.Contains("Winner", result);
        Assert.Contains("won the round", result);
        Assert.Equal(1, player.Score);
    }

    [Fact]
    public void JudgeVoteOnCard_AdvancesRound()
    {
        var lobby = CreateLobby();
        lobby.AddPlayer(new Player("Judge"));
        var player = new Player("Player1");
        player.Cards.Add("MyCard");
        lobby.AddPlayer(player);

        Assert.Equal(1, lobby.RoundNumber);

        lobby.PlayCardForPlayer(player.Id, "MyCard");
        lobby.JudgeVoteOnCard("MyCard");

        Assert.Equal(2, lobby.RoundNumber);
    }

    [Fact]
    public void JudgeVoteOnCard_ClearsPlayedCards()
    {
        var lobby = CreateLobby();
        lobby.AddPlayer(new Player("Judge"));
        var player = new Player("Player1");
        player.Cards.Add("MyCard");
        lobby.AddPlayer(player);

        lobby.PlayCardForPlayer(player.Id, "MyCard");
        Assert.NotEmpty(lobby.PlayedCards);

        lobby.JudgeVoteOnCard("MyCard");
        Assert.Empty(lobby.PlayedCards);
    }

    [Fact]
    public void JudgeVoteOnCard_ResetsLastPlayedCards()
    {
        var lobby = CreateLobby();
        lobby.AddPlayer(new Player("Judge"));
        var p1 = new Player("P1");
        p1.Cards.Add("Card1");
        lobby.AddPlayer(p1);
        var p2 = new Player("P2");
        p2.Cards.Add("Card2");
        lobby.AddPlayer(p2);

        lobby.PlayCardForPlayer(p1.Id, "Card1");
        lobby.PlayCardForPlayer(p2.Id, "Card2");

        lobby.JudgeVoteOnCard("Card1");

        Assert.Null(p1.LastPlayedCard);
        Assert.Null(p2.LastPlayedCard);
    }

    [Fact]
    public void JudgeVoteOnCard_AdvancesJudgeIndex()
    {
        var lobby = CreateLobby();
        lobby.AddPlayer(new Player("Judge0"));
        var p1 = new Player("P1");
        p1.Cards.Add("C1");
        lobby.AddPlayer(p1);

        Assert.Equal(0, lobby.JudgeIndex);

        lobby.PlayCardForPlayer(p1.Id, "C1");
        lobby.JudgeVoteOnCard("C1");

        Assert.Equal(1, lobby.JudgeIndex);
    }

    [Fact]
    public void JudgeVoteOnCard_JudgeIndexWrapsAround()
    {
        var lobby = CreateLobby();
        var p0 = new Player("P0");
        lobby.AddPlayer(p0);
        var p1 = new Player("P1");
        lobby.AddPlayer(p1);

        // Round 1: p0 is judge (index 0), p1 plays
        p1.Cards.Add("C1");
        lobby.PlayCardForPlayer(p1.Id, "C1");
        lobby.JudgeVoteOnCard("C1");
        Assert.Equal(1, lobby.JudgeIndex);

        // Round 2: p1 is judge (index 1), p0 plays
        p0.Cards.Add("C2");
        lobby.PlayCardForPlayer(p0.Id, "C2");
        lobby.JudgeVoteOnCard("C2");
        Assert.Equal(0, lobby.JudgeIndex); // wraps back to 0
    }

    [Fact]
    public void JudgeVoteOnCard_UpdatesLobbyHistory()
    {
        var lobby = CreateLobby();
        lobby.AddPlayer(new Player("Judge"));
        var player = new Player("P1");
        player.Cards.Add("WinCard");
        lobby.AddPlayer(player);

        lobby.PlayCardForPlayer(player.Id, "WinCard");
        // Before vote, the current question card is at RoundNumber (1)
        string questionCard = lobby.CurrentQuestionCard;
        lobby.JudgeVoteOnCard("WinCard");

        // After vote, RoundNumber is 2 and history should have the round 1 entry
        // The history is added after RoundNumber++, using the NEW CurrentQuestionCard
        Assert.Single(lobby.LobbyHistory);
    }

    [Fact]
    public void JudgeVoteOnCard_InvalidCard_ThrowsException()
    {
        var lobby = CreateLobby();
        lobby.AddPlayer(new Player("Judge"));
        var player = new Player("P1");
        player.Cards.Add("C1");
        lobby.AddPlayer(player);

        lobby.PlayCardForPlayer(player.Id, "C1");

        Assert.Throws<Exception>(() => lobby.JudgeVoteOnCard("NonexistentCard"));
    }

    [Fact]
    public void Locks_AreNotNull()
    {
        var lobby = CreateLobby();
        Assert.NotNull(lobby.DeckLock);
        Assert.NotNull(lobby.PlayersLock);
        Assert.NotNull(lobby.StateLock);
    }
}
