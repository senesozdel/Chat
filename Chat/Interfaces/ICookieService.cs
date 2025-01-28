namespace Chat.Interfaces
{
    public interface ICookieService
    {
        void SetCookie(string key, string value, int? expireTimeInMinutes = null);
        string? GetCookie(string key);
        void DeleteCookie(string key);
    }
}
