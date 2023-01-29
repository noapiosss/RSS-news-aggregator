using System.Threading.Tasks;

namespace Api.Services.Interfaces
{
    public interface IAggregator
    {
        public Task AggregateAsync();
    }
}