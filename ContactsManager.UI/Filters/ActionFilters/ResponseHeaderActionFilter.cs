using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
    public class ResponseHeaderFilterFactoryAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;
        public string? Key { get; set; }
        public string? Value { get; set; }
        public int Order { get; set; }

        public ResponseHeaderFilterFactoryAttribute(string key, string value, int order)
        {
            Key = key;
            Value = value;
            Order = order;
        }
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var filter = serviceProvider.GetRequiredService<ResponseHeaderActionFilter>();
            filter.Key = Key;
            filter.Value = Value;
            filter.Order = Order;

            // return filter object
            return filter;
        }
    }
    public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<ResponseHeaderActionFilter> _logger;
        public string? Key { get; set; }
        public string? Value { get; set; }
        public int Order { get; set; }

        //public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger, string key, string value, int order)
        //{
        //    _logger = logger;
        //    Key = key;
        //    Value = value;
        //    Order = order;
        //}

        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger)
        {
            _logger = logger;
        }

        //public void OnActionExecuted(ActionExecutedContext context)
        //{
        //    _logger.LogInformation("{FilterName}.{MethodName}", nameof(ResponseHeaderActionFilter), nameof(OnActionExecuted));

        //    context.HttpContext.Response.Headers[Key] = Value;
        //}

        //public void OnActionExecuting(ActionExecutingContext context)
        //{
        //    _logger.LogInformation("{FilterName}.{MethodName}", nameof(ResponseHeaderActionFilter), nameof(OnActionExecuting));
        //}

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Before logic here
            _logger.LogInformation("{FilterName}.{MethodName} method - before", nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));
            await next(); // calls the subsequent filter or action method

            // After logic here

            _logger.LogInformation("{FilterName}.{MethodName} method - after", nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

            context.HttpContext.Response.Headers[Key] = Value;
        }
    }
}
