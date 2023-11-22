using Microsoft.Extensions.FileProviders;
var builder = WebApplication.CreateBuilder(args);

GameServer gs = new GameServer("C:\\SourcePersonal\\New folder\\bedlam-digital\\web\\cards\\");
builder.Services.AddSingleton<GameServer>(gs);

var app = builder.Build();
app.UseHttpsRedirection();

// Enable static content served from the web folder 
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider("C:\\SourcePersonal\\New folder\\bedlam-digital\\web\\"),
});

app.MapGet("api/Lobby/StartNewLobby/{PlayerName}", (string PlayerName) =>
{
    string GameId = gs.StartNewLobby(PlayerName); 
    // Join a lobby (with the specified guid as GameId) and add the current PlayerName to it if it doesn't exist already    
    Player pl = gs.AddPlayerToLobby(GameId, PlayerName);
    pl.PlayingInGame = GameId;
    return pl;
});

app.MapGet("/api/Lobby/JoinExistingLobby/{GameId}/{PlayerName}", (string GameId, string PlayerName) =>
{
    // Join a lobby (with the specified guid as GameId) and add the current PlayerName to it if it doesn't exist already    
    return gs.AddPlayerToLobby(GameId, PlayerName);
});

app.MapGet("/api/Lobby/GetLobbyData/{GameId}", (string GameId) =>
{
    // Return the lobby data for the game
    return gs.Lobbies[GameId].CloneForMultiplayerSafety();
});

app.MapGet("/api/Lobby/GetLobbies", () =>
{
    // Return the lobby data for the game
    return LobbyList.GenerateList(gs);

});

// play a white card
app.MapGet("api/Game/PlayWhiteCard/{GameId}/{PlayerName}/{WhiteCardId}", (string GameId, string PlayerName, string WhiteCardId) =>
{
    // Play a white card, return the status
    gs.Lobbies[GameId].PlayCard(WhiteCardId, PlayerName); 
    return "OK";
});


app.MapGet("/api/Game/VoteOnWinner/{GameId}/{WinningPlayer}", (string GameId, string WinningPlayer) =>
{
    // Add a vote for the winning card, return the status
    gs.Lobbies[GameId].Players.Where( x => x.Name == WinningPlayer).FirstOrDefault().VotedForInRound();
    return "OK";
});

app.Run();