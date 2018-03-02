namespace Lykke.Service.HFT.Core.Services.ApiKey
{
    public interface ISessionCache
    {
        long[] GetSessionIds(string clientId);
        void AddSessionId(string token, long sessionId);
        bool TryRemoveSessionId(long sessionId);
    }
}
