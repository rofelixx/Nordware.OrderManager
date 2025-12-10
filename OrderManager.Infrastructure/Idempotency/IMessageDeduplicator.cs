using System.Collections.Concurrent;

namespace OrderManager.Infrastructure.Idempotency
{
    public interface IMessageDeduplicator
    {
        Task<bool> MarkProcessedIfNotExistsAsync(string messageId);
    }

    public class InMemoryMessageDeduplicator : IMessageDeduplicator
    {
        private static readonly ConcurrentDictionary<string, DateTime> _processed = new();

        public Task<bool> MarkProcessedIfNotExistsAsync(string messageId)
        {
            // Tenta adicionar; se já existir, retorna false
            var added = _processed.TryAdd(messageId, DateTime.UtcNow);
            return Task.FromResult(added);
        }
    }
}
