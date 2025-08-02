namespace TeleCasino.BaccaratGameService.Models;

public class BaccaratResult
{
    public required string Id { get; set; }
    public int Wager { get; set; }
    public decimal Payout { get; set; }
    public decimal NetGain { get; set; }
    public required string VideoFile { get; set; }
    public bool Win { get; set; }

    // game mechanics    
    public BaccaratBetType BetType { get; set; }
    public List<string> PlayerCards { get; set; } = new();
    public List<string> BankerCards { get; set; } = new();
    public int PlayerTotal { get; set; }
    public int BankerTotal { get; set; }
    public BaccaratBetType Winner { get; set; }
    public int GameSessionId { get; set; }
}