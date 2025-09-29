using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OpenTelemetry_Toy_Project;

public class DiceController: ControllerBase
{
    private ILogger<DiceController> _logger;

    public DiceController(ILogger<DiceController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/rolldice")]
    public List<int> RollDice(string player, int? rolls)
    {
        if (!rolls.HasValue)
        {
            _logger.LogError("Missing rolls parameter");
            throw new HttpRequestException("Missing rolls parameter", null, HttpStatusCode.BadRequest);
        }
        
        var result = new Dice(1, 6).RollTheDice(rolls.Value);

        if (string.IsNullOrEmpty(player))
        {
            _logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
        }
        else
        {
            _logger.LogInformation("{player} is rolling the dice: {result}", player, result);
        }
        
        return result;
    }
}