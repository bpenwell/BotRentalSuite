namespace Utilities
{
    public interface ISaveable
    {
        string SaveFileName { get; }

        void Save();

        void Load();
    }
}
