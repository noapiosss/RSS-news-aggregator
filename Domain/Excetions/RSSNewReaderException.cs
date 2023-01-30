using System;
using Contracts.Http;

namespace Domain.Excetions
{
    public class RSSNewsReaderException : Exception
    {
        public ErrorCode ErrorCode { get; }

        public RSSNewsReaderException(ErrorCode errorCode) : this(errorCode, null)
        {

        }

        public RSSNewsReaderException(ErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

    }
}