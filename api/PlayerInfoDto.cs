public class PlayerInfoDto
{
    public PlayerInfoDto() 
    {
        Name = "";
        Score = 0;
        LastPlayedCard = "";
    }
    public PlayerInfoDto(Player plr)
    {
        Name = plr.Name;
        Score = plr.Score;
        LastPlayedCard = plr.LastPlayedCard;
    }
    public string Name { get; set; }
    public int Score { get; set; }
    public string LastPlayedCard { get; set; }
}