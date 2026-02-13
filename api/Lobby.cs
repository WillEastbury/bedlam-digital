using Microsoft.Net.Http.Headers;
using System.Text.Json;

public class Lobby
{
    private static readonly string HighScoresFile = Path.Combine(Environment.CurrentDirectory, "highscores.json");
    private static readonly object HighScoresLock = new();
    public static List<GameResult> GameHistory { get; } = new();
    private static readonly object GameHistoryLock = new();
    public static int MaxPlayers = 8;
    public static int MaxRounds = 8;
    public static int CardsDealtPerPlayer = 8;
    public string Id { get; private set;}
    public List<Player> Players { get; private set;} = new List<Player>();
    public List<string> QuestionDeck { get; private set;}
    public List<string> AnswerDeck { get; private set;}
    public List<string> PlayedCards { get; private set;} = new List<string>();
    public List<ChatMessage> ChatMessages { get; private set;} = new List<ChatMessage>();
    public int SpectatorCount { get; set; } = 0;
    public Dictionary<string,string> LobbyHistory {get;set;} = new();
    public int RoundNumber {get; set;} = 1;
    public int JudgeIndex { get; set;}
    public string CurrentQuestionCard => QuestionDeck[RoundNumber];
    public bool IsLocked { get; set; } = false;
    public List<string> CardPacks { get; private set; } = new List<string> { "Core", "MSFT-SP1", "MSFT-SP2", "Update1" };
    public object DeckLock { get; } = new object();
    public object PlayersLock { get; } = new object();
    public object StateLock { get; } = new object();
    public Lobby(string id, List<string> cardPacks = null)
    {
        Id = id;
        if (cardPacks != null && cardPacks.Count > 0) CardPacks = cardPacks;
        QuestionDeck = ShuffleCards(GetQuestionCards());
        AnswerDeck = ShuffleCards(GetAnswerCards());
        JudgeIndex = 0;

     }
    List<string> GetQuestionCards() => new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Cards-PNG")).GetFiles("*Black*", new EnumerationOptions() {RecurseSubdirectories = true}).Select(e => e.Name.Replace(".png","")).Where(c => CardPacks.Any(p => c.StartsWith(p + "-"))).ToList();
    List<string> GetAnswerCards() => new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Cards-PNG")).GetFiles("*White*", new EnumerationOptions() {RecurseSubdirectories = true}).Select(e => e.Name.Replace(".png","")).Where(c => CardPacks.Any(p => c.StartsWith(p + "-"))).ToList();
    List<string> ShuffleCards(List<string> cards) => cards.OrderBy(_ => Guid.NewGuid()).ToList();
    public void AddPlayer(Player player) 
    {
        lock (PlayersLock)
        {
            Players.Add(player);
        }
    }
    public bool PlayCardForPlayer(string playerId, string cardUrl)
    {
        lock (PlayersLock)
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
            
            // Lock the lobby once a card is played
            if (!IsLocked) IsLocked = true;

            // set the lastplayed card for the player
            player.LastPlayedCard = cardUrl;
            return true;
        }
    }
    public string JudgeVoteOnCard(string cardUrl)
    {
        lock (StateLock)
        {
            lock (PlayersLock)
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
                foreach (var player in Players.ToList())
                {
                    player.LastPlayedCard = null;
                }
                JudgeIndex ++;
                if (JudgeIndex >= Players.Count) {
                    // if the judge index is greater than the number of players, reset it to 0
                    JudgeIndex = 0;
                }
                LobbyHistory.Add(CurrentQuestionCard, cardUrl);

                // Save high score at end of game (round > 12)
                if (RoundNumber > 12)
                {
                    var topPlayer = Players.OrderByDescending(p => p.Score).First();
                    SaveHighScore(new HighScore(topPlayer.Name, topPlayer.Score, DateTime.UtcNow, Id));
                    SaveGameResult(this);
                }

                return Winner.Name + " won the round! with Card <a href='/Card/" + cardUrl + " '>this card </a>, they now have " + Winner.Score + " points!";
            }
        }
    }
    public static void SaveHighScore(HighScore score)
    {
        lock (HighScoresLock)
        {
            var scores = LoadHighScores();
            scores.Add(score);
            scores = scores.OrderByDescending(s => s.Score).Take(10).ToList();
            File.WriteAllText(HighScoresFile, JsonSerializer.Serialize(scores));
        }
    }
    public static List<HighScore> LoadHighScores()
    {
        lock (HighScoresLock)
        {
            if (!File.Exists(HighScoresFile)) return new List<HighScore>();
            try { return JsonSerializer.Deserialize<List<HighScore>>(File.ReadAllText(HighScoresFile)) ?? new List<HighScore>(); }
            catch { return new List<HighScore>(); }
        }
    }
    public static void SaveGameResult(Lobby lobby)
    {
        lock (GameHistoryLock)
        {
            GameHistory.Add(new GameResult
            {
                LobbyName = lobby.Id,
                Players = lobby.Players.Select(p => new PlayerScoreEntry { Name = p.Name, Score = p.Score }).ToList(),
                Date = DateTime.UtcNow
            });
            if (GameHistory.Count > 20) GameHistory.RemoveAt(0);
        }
    }
    public static List<GameResult> GetGameHistory()
    {
        lock (GameHistoryLock)
        {
            return GameHistory.OrderByDescending(g => g.Date).Take(20).ToList();
        }
    }
}
