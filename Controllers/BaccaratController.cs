using Microsoft.AspNetCore.Mvc;
using TeleCasino.BaccaratGameService.Models;
using TeleCasino.BaccaratGameService.Services.Interface;

namespace TeleCasino.BaccaratGameApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaccaratController : ControllerBase
    {
        private readonly IBaccaratGameService _baccaratGameService;

        public BaccaratController(IBaccaratGameService baccaratGameService)
        {
            _baccaratGameService = baccaratGameService;
        }

        /// <summary>
        /// Plays a Baccarat game and returns the result with a generated video file path.
        /// </summary>
        /// <param name="wager">Amount wagered (must be one of: 0.05, 0.1, 0.5, 1, 2, 5, 10, 25, 50).</param>
        /// <param name="bet">Player's bet type ("player", "banker", "tie").</param>
        /// <param name="gameSessionId">Game session identifier.</param>
        [HttpPost("play")]
        [ProducesResponseType(typeof(BaccaratResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PlayGame(
            [FromQuery] decimal wager,
            [FromQuery] string bet,
            [FromQuery] int gameSessionId)
        {
            var validWagers = new HashSet<decimal> { 0.05m, 0.1m, 0.5m, 1m, 2m, 5m, 10m, 25m, 50m };
            if (!validWagers.Contains(wager))
                return BadRequest("Wager must be one of: 0.05, 0.1, 0.5, 1, 2, 5, 10, 25, 50.");

            if (string.IsNullOrWhiteSpace(bet))
                return BadRequest("You must specify a bet type (player, banker, tie).");

            var allowedBets = new[] { "player", "banker", "tie" };
            var betNorm = bet.Trim().ToLowerInvariant();
            if (!allowedBets.Contains(betNorm))
                return BadRequest("Bet type must be 'player', 'banker', or 'tie'.");

            var result = await _baccaratGameService.PlayGameAsync(wager, betNorm, gameSessionId);

            return Ok(result);
        }
    }
}
