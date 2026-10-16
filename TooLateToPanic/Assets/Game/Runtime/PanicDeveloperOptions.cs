namespace TooLateToPanic
{
    public enum PanicDeveloperOption
    {
        InfiniteBudget,
        FreeMaterials,
        FreezePreparationTime
    }

    /// <summary>Process-local developer switches. Public callers can inspect but not mutate them.</summary>
    public sealed class PanicDeveloperOptions
    {
        public bool InfiniteBudget { get; private set; }
        public bool FreeMaterials { get; private set; }
        public bool FreezePreparationTime { get; private set; }

        internal void SetOption(PanicDeveloperOption option, bool enabled)
        {
            switch (option)
            {
                case PanicDeveloperOption.InfiniteBudget: InfiniteBudget = enabled; break;
                case PanicDeveloperOption.FreeMaterials: FreeMaterials = enabled; break;
                case PanicDeveloperOption.FreezePreparationTime: FreezePreparationTime = enabled; break;
            }
        }
    }
}
