using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Http;
using Domain.Excetions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    public class BaseCotroller : ControllerBase
    {
        private readonly ILogger _logger;
        protected BaseCotroller(ILogger logger)
        {
            _logger = logger;
        }

        protected async Task<IActionResult> SafeExecute(Func<Task<IActionResult>> action,
            CancellationToken cancellationToken)
        {
            try
            {
                return await action();
            }
            catch (RSSNewsReaderException rrse)
            {
                _logger.LogError(rrse, "RRSNewReader exception raised");

                ErrorResponse errorResponse = new()
                {
                    Code = rrse.ErrorCode,
                    Message = rrse.Message
                };

                return ToActionResult(errorResponse);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhanled exception raised");

                ErrorResponse errorResponse = new()
                {
                    Code = ErrorCode.InternalServerError,
                    Message = "Unhandled error"
                };

                return ToActionResult(errorResponse);
            }
        }

        protected IActionResult ToActionResult(ErrorResponse errorResponse)
        {
            return StatusCode((int)errorResponse.Code / 100, errorResponse);
        }
    }
}