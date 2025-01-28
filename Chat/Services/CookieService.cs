using Chat.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using Newtonsoft.Json;
public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetCookie(string key, string value, int? expireTimeInMinutes = null)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,  
            Secure = true,  
            SameSite = SameSiteMode.Strict 
        };

        if (expireTimeInMinutes.HasValue)
        {
            cookieOptions.Expires = DateTime.Now.AddMinutes(expireTimeInMinutes.Value);
        }

        _httpContextAccessor.HttpContext?.Response.Cookies.Append(key, value, cookieOptions);
    }

    public void SetCookieList<T>(string key, List<T> list, int? expireTimeInMinutes = null)
    {
        string json = JsonConvert.SerializeObject(list);
        SetCookie(key, json, expireTimeInMinutes);
    }

    public List<T>? GetCookieList<T>(string key)
    {
        // Çerezi oku
        string? json = GetCookie(key);
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonConvert.DeserializeObject<List<T>>(json);
    }

    public string? GetCookie(string key)
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[key];
    }

    public void DeleteCookie(string key)
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(key);
    }
}
