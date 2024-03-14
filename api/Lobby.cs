using Microsoft.Net.Http.Headers;

public class Lobby
{
    public static int MaxPlayers = 8;
    public static int MaxRounds = 8;
    public static int CardsDealtPerPlayer = 8;
    public string Id { get; private set;}
    public List<Player> Players { get; private set;} = new List<Player>();
    public List<string> QuestionDeck { get; private set;}
    public List<string> AnswerDeck { get; private set;}
    public List<string> PlayedCards { get; private set;} = new List<string>();
    public Dictionary<string,string> LobbyHistory {get;set;} = new();
    public int RoundNumber {get; set;} = 1;
    public int JudgeIndex { get; set;}
    public string CurrentQuestionCard => QuestionDeck[RoundNumber];
    public Lobby(string id)
    {
        Id = id;
        QuestionDeck = ShuffleCards(GetQuestionCards());
        AnswerDeck = ShuffleCards(GetAnswerCards());
        JudgeIndex = 0;

     }
    List<string> GetQuestionCards() => new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Cards-PNG")).GetFiles("*Black*", new EnumerationOptions() {RecurseSubdirectories = true}).Select(e => e.Name.Replace(".png","")).ToList();
    List<string> GetAnswerCards() => new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Cards-PNG")).GetFiles("*White*", new EnumerationOptions() {RecurseSubdirectories = true}).Select(e => e.Name.Replace(".png","")).ToList();
    List<string> ShuffleCards(List<string> cards) => cards.OrderBy(_ => Guid.NewGuid()).ToList();
    public void AddPlayer(Player player) => Players.Add(player);
    public bool PlayCardForPlayer(string playerId, string cardUrl)
    {
        // Firstly retrieve the player from the lobby
        var player = Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return false;
        // Check if the player is the judge
        if (Players.IndexOf(player) == JudgeIndex) return false;
        // Validity: Check if the player has the card in their hand
        if (!player.Cards.Contains(cardUrl)) return false;

        // Remove the card from the player's hand
        player.Cards.Remove(cardUrl);
        
        // Add the card to the played cards
        PlayedCards.Add(cardUrl);
        
        // set the lastplayed card for the player
        player.LastPlayedCard = cardUrl;
        return true;
    }
    public string JudgeVoteOnCard(string cardUrl)
    {
        Console.WriteLine("Voting: " + cardUrl); 
        // Look through the players in the lobby and find the one that played that card.
        foreach(Player p in Players)
        {
            Console.WriteLine("Player: " + p.Name + " " + p.LastPlayedCard + " vs. " + cardUrl + " " + (p.LastPlayedCard == cardUrl));
        }
        Player Winner = Players.FirstOrDefault(p => p.LastPlayedCard == cardUrl);
        if (Winner == null) throw new Exception("ERR: No Player Specified");
        Winner.WonRound();
        RoundNumber++;
        PlayedCards.Clear();
        // Reset all of the players for this lobby's last played cards
        foreach (var player in Players)
        {
            player.LastPlayedCard = null;
        }
        JudgeIndex ++;
        if (JudgeIndex >= Players.Count) {
            // if the judge index is greater than the number of players, reset it to 0
            JudgeIndex = 0;
        }
        LobbyHistory.Add(CurrentQuestionCard, cardUrl);

        return Winner.Name + " won the round! with Card <a href='/Card/" + cardUrl + " '>this card </a>, they now have " + Winner.Score + " points!";
    }
}
