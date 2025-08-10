namespace TeleCasino.BaccaratGameService.Models;

public class BaccaratResult
{
    public required string Id { get; set; }
    public decimal Wager { get; set; }
    public decimal Payout { get; set; }
    public decimal NetGain { get; set; }
    public required string VideoFile { get; set; }
    public bool Win { get; set; }

    // game mechanics    
    public BaccaratBetType BetType { get; set; }
    public List<string> PlayerCards { get; set; } = new();
    public List<string> BankerCards { get; set; } = new();
    public decimal PlayerTotal { get; set; }
    public decimal BankerTotal { get; set; }
    public BaccaratBetType Winner { get; set; }
    public int GameSessionId { get; set; }
}