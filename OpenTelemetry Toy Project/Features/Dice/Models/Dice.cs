using System.Collections.Generic;

namespace OpenTelemetry_Toy_Project.Features.Dice.Models;

public class Dice
{
    private readonly int _min;
    private readonly int _max;

    public Dice(int min, int max)
    {
        _min = min;
        _max = max;
    }

    public List<int> RollTheDice(int rolls)
    {
        var results = new List<int>();

        for (var i = 0; i < rolls; i++)
        {
            results.Add(RollOnce());
        }

        return results;
    }

    private int RollOnce()
    {
        return Random.Shared.Next(_min, _max + 1);
    }
}
