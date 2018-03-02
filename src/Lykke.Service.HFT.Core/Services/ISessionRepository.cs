namespace Lykke.Service.HFT.Core.Services
{
    public interface ISessionRepository
    {
        long[] GetSessionIds(string clientId);
        void AddSessionId(string token, long sessionId);
        bool TryRemoveSessionId(long sessionId);
    }
}
