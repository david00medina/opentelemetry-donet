using System.Collections.Generic;

namespace OpenTelemetry_Toy_Project.Features.Dice.Controllers;

public interface IDiceController
{
    List<int> RollDice(string player, int? rolls);
}
