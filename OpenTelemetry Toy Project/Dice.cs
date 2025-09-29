namespace OpenTelemetry_Toy_Project;

public class Dice
{
    private int min;
    private int max;

    public Dice(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public List<int> RollTheDice(int rolls)
    {
        List<int> results = new List<int>();

        for (int i = 0; i < rolls; i++)
        {
            results.Add(RollOnce());
        }
        
        return results;
    }

    private int RollOnce()
    {
        return Random.Shared.Next(min, max + 1);
    }
}