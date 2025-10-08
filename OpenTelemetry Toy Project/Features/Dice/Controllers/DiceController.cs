using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenTelemetry_Toy_Project.Features.Dice.Models;

namespace OpenTelemetry_Toy_Project.Features.Dice.Controllers;

[ApiController]
[Route("[controller]")]
public class DiceController : ControllerBase, IDiceController
{
    private readonly ILogger<DiceController> _logger;

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

        var result = new Models.Dice(1, 6).RollTheDice(rolls.Value);

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
