namespace Chat.Cache
{
    public interface ICacheService
    {
        T Get<T>(string key);
        void Set<T>(string key, T value, int durationInMinutes = 60);
        void Remove(string key);
        bool Exists(string key);
        void Clear();
    }
}
