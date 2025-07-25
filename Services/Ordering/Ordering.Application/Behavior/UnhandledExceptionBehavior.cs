﻿using MediatR;
using Microsoft.Extensions.Logging;

namespace Ordering.Application.Behavior
{
    public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TResponse> _logger;

        public UnhandledExceptionBehavior(ILogger<TResponse> logger)
        {
            _logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;
                _logger.LogError(ex, $"Unhandled exception occurred with Request Name: {requestName}, {request}");
                throw;
            }
        }
    }
}
