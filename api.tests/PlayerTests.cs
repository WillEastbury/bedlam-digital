using System.IdentityModel.Tokens.Jwt;
using Xunit;

public class PlayerTests
{
    [Fact]
    public void Constructor_SetsNameAndDefaults()
    {
        var player = new Player("Alice");

        Assert.Equal("Alice", player.Name);
        Assert.Equal(0, player.Score);
        Assert.NotNull(player.Id);
        Assert.NotEmpty(player.Id);
        Assert.Empty(player.Cards);
    }

    [Fact]
    public void Constructor_GeneratesUniqueIds()
    {
        var p1 = new Player("A");
        var p2 = new Player("B");

        Assert.NotEqual(p1.Id, p2.Id);
    }

    [Fact]
    public void WonRound_IncrementsScore()
    {
        var player = new Player("Alice");
        Assert.Equal(0, player.Score);

        player.WonRound();
        Assert.Equal(1, player.Score);

        player.WonRound();
        Assert.Equal(2, player.Score);
    }

    [Fact]
    public void GenerateJwtToken_ReturnsValidToken()
    {
        var player = new Player("Alice");
        string token = player.GenerateJwtToken(player.Id, player.Name, "TestLobby");

        Assert.False(string.IsNullOrEmpty(token));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(player.Id, jwt.Claims.First(c => c.Type == "PlayerId").Value);
        Assert.Equal("Alice", jwt.Claims.First(c => c.Type == "PlayerName").Value);
        Assert.Equal("TestLobby", jwt.Claims.First(c => c.Type == "LobbyId").Value);
        Assert.Equal("BedlamServer", jwt.Issuer);
    }

    [Fact]
    public void GenerateJwtToken_DifferentPlayersHaveDifferentKeys()
    {
        var p1 = new Player("Alice");
        var p2 = new Player("Bob");

        string t1 = p1.GenerateJwtToken(p1.Id, p1.Name, "Lobby1");
        string t2 = p2.GenerateJwtToken(p2.Id, p2.Name, "Lobby1");

        Assert.NotEqual(t1, t2);
        // Each player has a unique secret key
        Assert.False(p1.secretKey.SequenceEqual(p2.secretKey));
    }

    [Fact]
    public void SecretKey_IsLongEnough()
    {
        var player = new Player("Alice");
        // Two GUIDs concatenated should be ~72 bytes in UTF8
        Assert.True(player.secretKey.Length >= 64);
    }

    [Fact]
    public void LastPlayedCard_InitializesToEmptyString()
    {
        var player = new Player("Test");
        Assert.Equal("", player.LastPlayedCard);
    }

    [Fact]
    public void Cards_CanBeModified()
    {
        var player = new Player("Test");
        player.Cards.Add("Card1");
        player.Cards.Add("Card2");

        Assert.Equal(2, player.Cards.Count);
        Assert.Contains("Card1", player.Cards);
    }
}
