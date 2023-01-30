using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Domain.Base
{
    internal abstract class BaseHandler<TRequest, TResult> : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        protected readonly ILogger Logger;
        private readonly string _name;
        protected BaseHandler(ILogger logger)
        {
            Logger = logger;
            _name = GetType().Name;
        }

        public async Task<TResult> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogDebug("Start to execture {Type}. Input {@Input}",
                    _name,
                    request);

                TResult result = await HandleInternal(request, cancellationToken);

                Logger.LogDebug("Executed {Type}, Result {@Input}",
                    _name,
                    result);

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Exception raised. Input: {@Input}", request);
                throw;
            }
        }

        protected abstract Task<TResult> HandleInternal(TRequest request, CancellationToken cancellationToken);
    }
}