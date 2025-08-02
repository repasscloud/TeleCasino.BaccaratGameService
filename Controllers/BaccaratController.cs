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
        /// <param name="wager">Amount wagered (must be 1, 2, or 5).</param>
        /// <param name="bet">Player's bet type ("player", "banker", "tie").</param>
        /// <param name="gameSessionId">Game session identifier.</param>
        [HttpPost("play")]
        [ProducesResponseType(typeof(BaccaratResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PlayGame(
            [FromQuery] int wager,
            [FromQuery] string bet,
            [FromQuery] int gameSessionId)
        {
            var validWagers = new[] { 1, 2, 5 };
            if (!validWagers.Contains(wager))
                return BadRequest("Wager must be either 1, 2, or 5.");

            if (string.IsNullOrWhiteSpace(bet))
                return BadRequest("You must specify a bet type (player, banker, tie).");

            var allowedBets = new[] { "player", "banker", "tie" };
            if (!allowedBets.Contains(bet.Trim().ToLowerInvariant()))
                return BadRequest("Bet type must be 'player', 'banker', or 'tie'.");

            var result = await _baccaratGameService.PlayGameAsync(wager, bet.Trim().ToLowerInvariant(), gameSessionId);

            return Ok(result);
        }
    }
}
