using System.Text.Json; 
public class Player
{
    public Player(string Name) : base()
    {
        this.Name = Name;
        // Generate a new Guid and assign it to PlayerGuidToken
        this.PlayerGuidToken = Guid.NewGuid().ToString();

    }
    public string PlayerGuidToken {get;set;}
    public string Name {get;set;}
    public int Score {get;set;} = 0;
    public int VotesInCurrentRound = 0;
    public string PlayingInGame {get; set;} = "";

    public List<string> WhiteCardsHeldInHand {get;set;} =  new List<string>();

    public void VotedForInRound() => VotesInCurrentRound ++;

    public void WonRound() {
        VotesInCurrentRound = 0; 
        Score ++;
    }

    public void LostRound() 
    {
        VotesInCurrentRound = 0;
    }

    public Player CloneForMultiplayerSafety() 
    {
        // Serialize this object to a string using System.Text.Json
        string serialized = JsonSerializer.Serialize<Player>(this);
        
        // Deserialize it again to a temp value 
        Player tempPlayer = JsonSerializer.Deserialize<Player>(serialized);

        // Nuke the PlayerGuidToken and the WhiteCardsHeldInHand
        tempPlayer.PlayerGuidToken = "Redacted - nice try ;)"; 
        tempPlayer.WhiteCardsHeldInHand = new List<string>();
        tempPlayer.WhiteCardsHeldInHand.Add("Redacted - nice try ;)");
        
        // Return the temp value
        return tempPlayer;


    }
}

