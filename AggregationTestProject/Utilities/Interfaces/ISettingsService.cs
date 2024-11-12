namespace AggregationTestProject.Utilities.Interfaces
{
    public interface ISettingsService<T> 
        where T : class
    {
        T GetSettings();
        void SetSettings(T settings);
    }
}
