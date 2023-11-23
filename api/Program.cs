using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var players = new List<Player>();
var lobbies = new List<Lobby>();
var names = new List<string>() {"Janus (Windows 3.1 and MSDOS 5)", "Anaheim (Microsoft Edge)", "Astro (MS DOS 6)", "Bandit (Schedule Plus)", "Sparta (Windows for Workgroups 3.1)", "Snowball (Windows for Workgroups 3.11)", "Chicago (Windows 95)", "Memphis (Windows 98)", "Millennium (Windows ME)", "Razzle (Windows NT 3.1)", "Daytona (Windows NT 3.5)", "Tukwila (Windows NT 4)", "Cairo", "[] (Windows 2000)", "Whistler (Windows XP)", "Longhorn (Windows Vista)", "Vienna (Windows 7)", "Midori (Application Shim for Singularity, used in SQLPAL)", "Blue (Windows 8.1)", "Threshold (Windows 10)", "Redstone (Windows 10 Creators Update)", "Sun Valley (Windows 11)", "Hudson Valley (Windows 12?)", "Pegasus (Windows CE 1.0)", "Rapier (Windows Pocket PC 2000)", "Maldives (Windows Phone 7)", "Red Dog (Azure)", "Singularity (MSR Managed OS)", "Denali (SQL Server 2012)", "Thunder (Visual Basic 1.0)", "Bullet (MS Mail 3.0)", "Opus (Word for Windows v1.0)", "Wren (Outlook)", "Utopia (Bob UI)", "Marvel (MSN)", "Argo (Zune Player)", "KittyHawk (Visual Studio Lightswitch)", "DirectX Box (Xbox)", "Natal (Kinect)", "Natick (Underwater DC Pod)", "Roslyn (.net compiler platform)", "Aspen (Visual Studio 6.0)", "Rainier (Visual Studio.NET 2002)", "Whidbey (Visual Studio 2005)" };
var app = builder.Build();

// Non-authenticated endpoints first
// ---------------------------------

// GET => Serve the root 
app.MapGet("/", () =>
{
    Console.Write(".");
    var filePath = Path.Combine(Environment.CurrentDirectory, "html", "index.html"); // Replace with the actual path to your SPA's index.html
    return Results.File(filePath, "text/html");
});

// GET => Serve the root 
app.MapGet("/Restart", () =>
{
    Console.WriteLine(".Restart Request Received");

    // Clear all lobbies and players
    lobbies.Clear();
    players.Clear();

    return Results.Ok("Game Engine Restarted!");
});

// GET => Serve the root 
app.MapGet("/Card/{CardId}", (string CardId) =>
{
    Console.Write(".");
    return Results.File(Path.Combine(Environment.CurrentDirectory, "Cards-PNG", CardId + ".png"), "image/png");
});

// GET /Lobbies => Returns a list of all lobbies
app.MapGet("/Lobbies", () =>
{
    Console.Write("L");
    Lobby lobby = CreateAndAddNewLobbyIfNoSpace();
    return Results.Ok(lobbies.Select(e => new LobbyDto(e)).ToList());
});

// GET /Login/{LobbyId}{PlayerName} => Creates a new player, joins a lobby (if one does not exist with the correct number of players one is created) and returns a JWT token
app.MapGet("/Login/{lobbyId}/{playerName}", (string playerName, string lobbyId) =>
{
    Console.Write("U");
    if (players.FirstOrDefault(p => p.Name == playerName) != null) return Results.Conflict("Player name already exists.");
    var player = new Player(playerName);
    players.Add(player);
    Lobby lobby = CreateAndAddNewLobbyIfNoSpace();
    player.Cards = lobby.AnswerDeck.Take(Lobby.CardsDealtPerPlayer).ToList();
    foreach (var card in player.Cards) lobby.AnswerDeck.Remove(card);
    lobby.AddPlayer(player);
    player.Token = player.GenerateJwtToken(player.Id, player.Name, lobby.Id);
    return Results.Ok(player.Token);
});

// GET /MyLobby => Returns the content of the lobby with the specified id via a dto to mask keys etc.
app.MapGet("/MyLobby", (HttpContext context) =>
{

    // Check the user has permissions to this lobby
    if (!SetAuth(context))
    {
        return Results.BadRequest("Err: Lobby - Not authed");
    }

    Lobby lobby = lobbies.FirstOrDefault(l => l.Id == context.User.Claims.FirstOrDefault(e => e.Type == "LobbyId").Value);

    if (lobby == null)
    {
        return Results.NotFound($"Lobby not found, but for your valid issued token ?!?!!? This should not occurr, please let Will.E know how you got this far ?! ;)");
    }

    return Results.Ok(new MyLobbyDto(lobby));
});

// GET /Cards/} => Retrieve the list of dealt cards in your hand for the player, and the white card in play
app.MapGet("/Cards", (HttpContext context) =>
{
    // Check the user has permissions to a lobby
    if (!SetAuth(context))
    {
        return Results.BadRequest("ERR: /Cards Not authed");
    }

    string lobbyId = context.User.Claims.Where(e => e.Type == "LobbyId").FirstOrDefault().Value;
    string playerId = context.User.Claims.Where(e => e.Type == "PlayerId").FirstOrDefault().Value;

    var lobby = lobbies.FirstOrDefault(l => l.Id == lobbyId);
    if (lobby == null)
    {
        return Results.NotFound("Lobby not found.");
    }

    if (lobby.Players.Where(p => p.Id == playerId).FirstOrDefault() == null)
    {
        return Results.NotFound("Player not found in lobby.");
    }

    // Find player and card, perform the player's turn logic
    CardsDto cd = new CardsDto()
    {
        QuestionCard = lobby.CurrentQuestionCard,
        AnswerCards = lobby.Players.Where(p => p.Id == playerId).FirstOrDefault().Cards
    };
    return Results.Ok(cd);
});

// POST /Lobbies/Play/{CardId} => Plays a card for a player into a lobby, returns a success message
app.MapPost("/Lobbies/Play/{cardId}", (HttpContext context, string cardId) =>
{
    // Check the user has permissions to a lobby
    if (!SetAuth(context))
    {
        return Results.BadRequest("ERR: Lobbies/Play/{Card} Notauthed.");
    }

    string lobbyId = context.User.Claims.Where(e => e.Type == "LobbyId").FirstOrDefault().Value;
    string playerId = context.User.Claims.Where(e => e.Type == "PlayerId").FirstOrDefault().Value;

    var lobby = lobbies.Where(l => l.Id == lobbyId).FirstOrDefault();
    if (lobby == null)
    {
        return Results.NotFound("ERR: Lobby not found.");
    }

    // Find player and card, perform the player's turn logic
    if (lobby.PlayCardForPlayer(playerId, cardId))
    {
        return Results.Ok(cardId + " Card played successfully for player:" + playerId);
    }
    else
    {
        return Results.BadRequest($"ERR: Card {cardId} not played successfully for player:{playerId}");
    
    }
    
});

app.MapGet("/Lobbies/Played", (HttpContext context) =>
{
    // Check the user has permissions to a lobby
    if (!SetAuth(context))
    {
        return Results.BadRequest("ERR: /Lobbies/Played Notauthed.");
    }

    string lobbyId = context.User.Claims.Where(e => e.Type == "LobbyId").FirstOrDefault().Value;
    string playerId = context.User.Claims.Where(e => e.Type == "PlayerId").FirstOrDefault().Value;

    var lobby = lobbies.Where(l => l.Id == lobbyId).FirstOrDefault();
    if (lobby == null)
    {
        return Results.NotFound("ERR: Lobby not found.");
    }

    return Results.Ok(lobby.PlayedCards);
});

// POST /Lobbies/Judge/{cardUrl} => Judge votes on a card, returns a success message
app.MapPost("/Lobbies/Judge/{cardUrl}", (HttpContext context, string cardUrl) =>
{

    if (!SetAuth(context))
    {
        return Results.BadRequest("/Lobbies/Judge/{Card} ERR: Notauthed.");
    }

    string lobbyId = context.User.Claims.Where(e => e.Type == "LobbyId").FirstOrDefault().Value;
    string playerId = context.User.Claims.Where(e => e.Type == "PlayerId").FirstOrDefault().Value;

    var lobby = lobbies.FirstOrDefault(l => l.Id == lobbyId);
    if (lobby == null)
    {
        return Results.NotFound("ERR: Lobby not found.");
    }
    string judging = lobby.JudgeVoteOnCard(cardUrl); // Judge votes on a card

    if (judging.StartsWith("ERR:"))
    {
        return Results.BadRequest(judging);
    }
    else
    {
        // Return text saying who won the round
        return Results.Ok(judging);
    }
});

app.Run();

string GetRandomName() {

    // Fetch the list of used lobby names 
    List<string> usedlobbynames = lobbies.Select(f => f.Id).ToList();
    // Return a random name from the list of names that are not in use
    return names.Except(usedlobbynames).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
};

bool SetAuth(HttpContext context, string Claim = null, string Value = null)
{
    string jwt = context.Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
    if (string.IsNullOrEmpty(jwt))
    {
        Console.WriteLine("ERR: No JWT token provided");
        return false;
    }

    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
    JwtSecurityToken jwtToken = tokenHandler.ReadToken(jwt) as JwtSecurityToken;
    SymmetricSecurityKey key;
    System.Security.Claims.Claim playerIdClaim;
    try
    {
        // Access PlayerId directly from the decoded token so we can lookup the correct key for the player
        playerIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "PlayerId");
        key = new SymmetricSecurityKey(players.Where(e => e.Id == playerIdClaim.Value).FirstOrDefault().secretKey);
    }
    catch (Exception ex)
    {
        // No PlayerId claim, or no matching playerid so this is not a valid player token or its for an old game
        Console.WriteLine("Exception FINDING Signing Key:" + ex.Message);
        return false;
    }

    if (!string.IsNullOrEmpty(jwt))
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Modify based on your issuer validation requirements
            ValidateAudience = false, // Modify based on your audience validation requirements
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };

        try
        {
            context.User = tokenHandler.ValidateToken(jwt, validationParameters, out _);
        }
        catch (SecurityTokenValidationException stex)
        {
            Console.WriteLine("Exception:" + stex.Message);
            context.User = null;
            return false;
        }
    }

    if (Claim == null) // No authZ required, just authN (set the context then exit with true)
    {
        // authNed
        Console.Write("Z");
        return true;
    }

    System.Security.Claims.Claim claim = context.User.Claims.Where(c => c.Type == Claim && c.Value == Value).FirstOrDefault();

    if (claim != null)
    {
        // AuthZ claim match
        Console.Write("N");
        return true;
    }

    Console.WriteLine($"ERR: Claim {Claim} with value {Value} NOT found for user {playerIdClaim.Value}");
    return false;
}
Lobby CreateAndAddNewLobbyIfNoSpace()
{   
    Lobby lobby = lobbies.FirstOrDefault(l => l.Players.Count < Lobby.MaxPlayers);
    // If there are no lobbies available, create one 
    if (lobby == null)
    {
        lobby = new Lobby(GetRandomName());
        lobbies.Add(lobby);
        Console.WriteLine("Created new lobby: " + lobby.Id);
    }
    return lobby;
}