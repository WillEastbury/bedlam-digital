using System.Text.Json;
public class Lobby
{
    // Give the lobby a name 
    private string[] lobbyNames = {"satya", "lisa", "bill", "scott", "jason", "chris", "gaurav", "melanie", "jean-philippe", "carol", "brad", "judson", "phil", "rajesh", "ryan", "kathleen", "kevin", "amy", "jelle", "will", "dennis", "amina", "anusha", "billy", "juergen", "cathy", "mary", "michael", "glen", "alejandra", "junior", "ric", "andy", "faisal", "umar", "ben", "thomas", "sam", "belkis", "maria", "graham", "tim", "cam", "michelle", "kashif", "donna", "anthony", "ricardo", "al", "josh"};
    
    public Lobby() {}
    public Lobby(string JudgeName) 
    {
        this.GameId = Guid.NewGuid().ToString();
        this.Judge = JudgeName;
        this.PlayedRounds = 0;

        // Give it a random display name by concatenating 2 strings from above with underscores 
        this.LobbyName = lobbyNames[new Random().Next(0, lobbyNames.Length)] + "_" + lobbyNames[new Random().Next(0, lobbyNames.Length)];
    }   

    public string Judge {get;set;}
    public string GameId {get;set;}
    public int PlayedRounds {get;set;} = 0;
    public int MaxPlayers {get;set;} = 6;
    public string LobbyName {get;set;}
    public string GameState {get;set;} = "Not Started"; // "Not Started", "Playing", "Voting", "Judging", "Completed";

    public int NumberOfPlayers => this.Players.Count;
    public bool IsFull => this.Players.Count == MaxPlayers; 

    public int PlayedCards {get;set;} = 0;
    public int VotedCards {get;set;} = 0; 

    public string CurrentBlackCard {get;set;}
    public Dictionary<string, string> CurrentWhiteCardStackFromPlayers {get;set;  }= new Dictionary<string, string>(); 
    public List<Player> Players {get;set;} = new List<Player>();

    protected internal List<string> WhiteCards {get;set;} = new List<string>();
    public List<string> BlackCards = new List<string>();
    
    public void StartNextRound() 
    {
        // Increment the winner's score (for the player that has most votes)
        List<Player> winners = Players.OrderByDescending(x => x.VotesInCurrentRound).Where(y => y.Score == Players.Max(z => z.Score)).ToList();

        foreach(Player thisWinner in winners)
        {
            thisWinner.WonRound(); 
        }

        foreach(Player thisLoser in losers)

        // Reset the votes for the round
        foreach (var player in Players)
        {
            player.VotesInCurrentRound = 0;
        }

        // Increment the round counter
        PlayedRounds ++;
        PlayedCards = 0;
        VotedCards = 0; 

        // Clear the played cards stack
        CurrentWhiteCardStackFromPlayers = new Dictionary<string,string>(); 
    
        // Rotate the judge to the next person in the list
        // If the judge is the last in the list, set the judge to the first in the list
        var judgeIndex = Players.FindIndex(x => x.Name == Judge);
        if (judgeIndex == Players.Count - 1)
        {
            Judge = Players[0].Name;
        }
        else
        {
            Judge = Players[judgeIndex + 1].Name;
        }

        CurrentBlackCard = this.BlackCards[PlayedRounds];
        GameState = "Playing"; 

    }

    public void PlayCard(string WhiteCard, string PlayerName) 
    {
        // Add to the played cards stack for the lobby
        CurrentWhiteCardStackFromPlayers.Add(PlayerName, WhiteCard);

        // Remove card from the player's hand
        Players.Where(x => x.Name == PlayerName).FirstOrDefault().WhiteCardsHeldInHand.Remove(WhiteCard);
        PlayedCards ++; 

        // Check if all of the cards have been played for this round
        if(PlayedCards == Players.Count)
        {
            // Start Voting 
            GameState = "Voting"; 
        }

        int minVotesNeededToWin = (Players.Count() / 2) + 1;
        
        if(VotedCards == Players.Count) // Everyone voted, move on 
        {
            // Detect and Mark the winner 
            StartNextRound(); 

        }
        else if(Players.OrderByDescending(p => p.VotesInCurrentRound).Where(e => e.VotesInCurrentRound >= minVotesNeededToWin).Count() > 0)
        // check if the winner is clear already as one player has more clear votes than the players left to vote
        {
            // mark the winner
            StartNextRound(); 
        }

    }

    public Lobby CloneForMultiplayerSafety() 
    {
        // Serialize this object to a string using System.Text.Json
        string serialized = JsonSerializer.Serialize<Lobby>(this);

        // Deserialize it again to a temp value 
        Lobby tempLobby = JsonSerializer.Deserialize<Lobby>(serialized);
        List<Player> redPlayers = new List<Player>(); 

        // Walk the list of Players and clone each of those
        foreach (Player p in tempLobby.Players) 
        {
            redPlayers.Add(p.CloneForMultiplayerSafety());
        }
        tempLobby.Players = redPlayers; 
        tempLobby.CurrentBlackCard = CurrentBlackCard; 

        // Return the temp value
        return tempLobby;
    }
}

