namespace HEAL.HeuristicLibContracts.Util;

public static class GenericExtensions
{
    extension<T>(T it)
    {
        public T With(Action<T> action)
        {
            action(it);

            return it;
        }
    }

    extension<T>(T? it)
    {
        public T NotNullOrThrow(Exception ex)
            => it ?? throw ex;
    }

    extension<T>(T it) where T : class
    {
        public void Do(Action<T> action)
            => action(it);
    }

    extension<T>(ref T it) where T : struct
    {
        public void Do(RefAction<T> action)
            => action(ref it);
    }
}

public delegate void RefAction<T>(ref T it);
