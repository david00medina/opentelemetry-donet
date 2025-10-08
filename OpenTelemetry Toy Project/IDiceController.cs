namespace OpenTelemetry_Toy_Project;

public interface IDiceController
{
    public List<int> RollDice(string player, int? rolls);
}