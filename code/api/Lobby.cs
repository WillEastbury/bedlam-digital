public class Lobby
{
    public static int MaxPlayers = 8;
    public static int MaxRounds = 8;
    public static int CardsDealtPerPlayer = 10;
    public string Id { get; private set;}
    public List<Player> Players { get; private set;} = new List<Player>();
    public List<string> QuestionDeck { get; private set;}
    public List<string> AnswerDeck { get; private set;}
    public List<string> PlayedCards { get; private set;} = new List<string>();
    public int RoundNumber {get; set;} = 01;
    public int JudgeIndex { get; set;}
    public string CurrentQuestionCard => QuestionDeck[RoundNumber];
    public Lobby(string id)
    {
        Id = id;
        QuestionDeck = ShuffleCards(GetQuestionCards());
        AnswerDeck = ShuffleCards(GetAnswerCards());
        JudgeIndex = new Random().Next(0, Players.Count);

     }
    List<string> GetQuestionCards() => new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Cards-PNG")).GetFiles("*Black*", new EnumerationOptions() {RecurseSubdirectories = true}).Select(e => e.Name.Replace(".png","")).ToList();
    List<string> GetAnswerCards() => new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Cards-PNG")).GetFiles("*White*", new EnumerationOptions() {RecurseSubdirectories = true}).Select(e => e.Name.Replace(".png","")).ToList();
    List<string> ShuffleCards(List<string> cards) => cards.OrderBy(_ => Guid.NewGuid()).ToList();
    public void AddPlayer(Player player) => Players.Add(player);
    public void PlayCardForPlayer(string playerId, string cardUrl)
    {
        // Firstly retrieve the player from the lobby
        var player = Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return;
        // Check if the player is the judge
        if (Players.IndexOf(player) == JudgeIndex) return;
        // Validity: Check if the player has the card in their hand
        if (!player.Cards.Contains(cardUrl)) return;
        // Remove the card from the player's hand
        player.Cards.Remove(cardUrl);
        // Add the card to the played cards
        PlayedCards.Add(cardUrl);
        // set the lastplayed card for the player
        player.LastPlayedCard = cardUrl;
    }
    public string JudgeVoteOnCard(string cardUrl)
    {
        // Look through the players in the lobby and find the one that played that card.
        Player Winner = Players.FirstOrDefault(p => p.Cards.Contains(cardUrl));
        if (Winner == null) throw new Exception("ERR: No Player Specified");
        Winner.WonRound();
        RoundNumber++;
        PlayedCards.Clear();
        // Reset all of the players for this lobby's last played cards
        foreach (var player in Players)
        {
            player.LastPlayedCard = null;
        }
        JudgeIndex = new Random().Next(0, Players.Count);
        return Winner.Name + " won the round! with Card <a href='/Card/" + cardUrl + " '>this card </a>, they now have " + Winner.Score + " points!";
    }
}