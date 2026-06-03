namespace HEAL.HeuristicLibContracts.Random;

public sealed class Rng(int? seed = null) : IRng
{
    private readonly System.Random random
        = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

    public int Next() => random.Next();
}
