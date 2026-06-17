namespace HEAL.HeuristicLibContracts.Random;

public interface IRng
{
    int Next();
    int Next(int maxValue);
}
