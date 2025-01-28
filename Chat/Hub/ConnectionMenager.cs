using System.Collections.Concurrent;

public static class ConnectionManager
{
    private static readonly ConcurrentDictionary<string, string> _connections = new ConcurrentDictionary<string, string>();

    public static void AddConnection(string userName, string connectionId)
    {
        _connections[userName] = connectionId;
    }

    public static string GetConnectionId(string userName)
    {
        return _connections.ContainsKey(userName) ? _connections[userName] : null;
    }

    public static void RemoveConnection(string userName)
    {
        _connections.TryRemove(userName, out _);
    }

    public static IEnumerable<string> GetAllConnectionIds()
    {
        return _connections.Values;
    }
}
