namespace HEAL.HeuristicLibContracts.Util;

public static class TaskExtensions
{
    extension(Task t)
    {
        public void Forget()
        {
            _ = t.ContinueWith(
                task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.Error.WriteLine(task.Exception);
                    }
                },
                TaskContinuationOptions.OnlyOnFaulted
            );
        }
    }
}
