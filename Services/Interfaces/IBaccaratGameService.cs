using TeleCasino.BaccaratGameService.Models;

namespace TeleCasino.BaccaratGameService.Services.Interface;

public interface IBaccaratGameService
{
    Task<BaccaratResult> PlayGameAsync(int wager, string betArg, int gameSessionId);
}