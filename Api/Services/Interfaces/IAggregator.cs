using System.Threading;
using System.Threading.Tasks;

namespace Api.Services.Interfaces
{
    public interface IAggregator
    {
        public Task AggregateAsync(CancellationToken cancellationToken);
        public Task DeleteSubscriblessFeed(CancellationToken cancellationToken);
    }
}