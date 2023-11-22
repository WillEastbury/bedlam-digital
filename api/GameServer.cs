public class GameServer
{
    public string CardPath = "C:\\SourcePersonal\\New folder\\bedlam-digital\\web\\cards\\";

    public Dictionary<string, Lobby> Lobbies = new Dictionary<string, Lobby>();
    public Dictionary<string, string> KickedPlayers = new Dictionary<string, string>();

    public List<string> whiteCardSet = new List<string>();
    public List<string> blackCardSet = new List<string>();

    public int DefaultMaxLobbySize = 10; 
    public int DefaultNumberOrRounds = 7; 

    // CTOR that can set an alternate card path 
    public GameServer(string cardPath = null) : base()
    {
        if (cardPath != null) this.CardPath = cardPath;

        // Load the cards from the file system
        LoadCards();
    }  

    public string StartNewLobby(string StartingPlayerName)
    {
        // Define a new guid and create the dictionary entry 
        Lobby newLobby = new Lobby(StartingPlayerName);

        newLobby.MaxPlayers = DefaultMaxLobbySize;
        newLobby.PlayedRounds = 0; 
        // Copy the deck of cards from the default lists and shuffle them 
        newLobby.WhiteCards = whiteCardSet.OrderBy(x => Guid.NewGuid()).ToList();
        newLobby.BlackCards = blackCardSet.OrderBy(x => Guid.NewGuid()).Take(DefaultNumberOrRounds + 1).ToList();
        newLobby.CurrentBlackCard = newLobby.BlackCards[newLobby.PlayedRounds];

        Lobbies.Add(newLobby.GameId, newLobby);
        return newLobby.GameId;
    }

    public Player AddPlayerToLobby(string GameId, string PlayerName)
    {
        // Create, deal and add the player to the lobby if it exists
        if (Lobbies.ContainsKey(GameId))
        {
            Player plr = new Player(PlayerName);
            
            // Deal the player cards off the top of the shuffled stack and then remove them from the deck 
            plr.WhiteCardsHeldInHand = Lobbies[GameId].WhiteCards.Take(DefaultNumberOrRounds).ToList();

            foreach (string card in plr.WhiteCardsHeldInHand)
            {
                Lobbies[GameId].WhiteCards.Remove(card);
            }

            Lobbies[GameId].Players.Add(plr);

            return plr;
        }
        else
        {
            throw new Exception($"Lobby {GameId} Not Found");
        }
    }

    public void KickPlayerFromLobby(string GameId, string KickedPlayerName, string Reason)
    {
        KickedPlayers.Add(KickedPlayerName, Reason); 
        Lobbies[GameId].Players.Remove(Lobbies[GameId].Players.Where(x => x.Name == KickedPlayerName).FirstOrDefault());
    }

    public Lobby GetLobbyData(string GameId)
    {
        return Lobbies[GameId].CloneForMultiplayerSafety();
    }
    
    public Lobby GetAllLobbies(string GameId)
    {
        return Lobbies[GameId].CloneForMultiplayerSafety();
    }

    public void LoadCards() 
    {
        // Look in CardPath and find all of the cards, adding any cards with the string 'White' in the name to the white pack and the same for 'Black'.
        // Firstly let's get the list of files in cardpath 
        List<string> files = Directory.GetFiles(CardPath).Select(x => Path.GetFileName(x)).ToList<string>();      

        // Now linq query across files to find ones that contain the string 'White' in the filename
        whiteCardSet = files.Where(x => x.Contains("White")).ToList();
        blackCardSet = files.Where(x => x.Contains("Black")).ToList();

    }
}
public class LobbyList : List<LobbyListing>
{
    public static LobbyList GenerateList(GameServer gs){
        LobbyList ll = new LobbyList();
        foreach (Lobby l in gs.Lobbies.Values)
        {
            ll.Add(new LobbyListing() { LobbyName = l.LobbyName, LobbyId = l.GameId, JudgeName = l.Judge, Players = l.Players.Select(x => x.Name).ToList() });
        }
        return ll;
    }

}
public class LobbyListing 
{
    public string LobbyName {get;set;}
    public string LobbyId {get;set;}
    public string JudgeName {get;set;}
    public List<string> Players {get;set;}
}