using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class Player
{
    public byte[] secretKey { get; private set;} = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + Guid.NewGuid().ToString());
    public string Id; 
    public string Name;
    public string LastPlayedCard;
    public int Score;
    public string Token;
    public List<string> Cards = new List<string>();
    public Player(string Name)
    {
        this.Id = Guid.NewGuid().ToString();
        this.Name = Name;
        this.Score = 0;
        this.LastPlayedCard = "";
    }
    public string GenerateJwtToken(string playerId, string playerName, string LobbyId)
    {
        var claims = new List<Claim>
        {
            new Claim("PlayerId", playerId),
            new Claim("PlayerName", playerName),
            new Claim("LobbyId", LobbyId)
        };

        var token = new JwtSecurityToken
        (
            "BedlamServer", // Replace with your issuer
            "BedlamServer", // Replace with your audience
            claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256)
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
    public void WonRound() => Score++;
}