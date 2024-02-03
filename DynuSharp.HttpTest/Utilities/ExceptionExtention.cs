using DynuSharp.Data;
using DynuSharp.Exceptions;
using System.Text.Json;

namespace DynuSharp.HttpTest.Utilities;
public static class ExceptionExtention
{
    public static object ToJsonObject(this Exception ex)
    {
        if (ex is DynuApiException dynuEx)
        {
            var apiError = new ApiError() { Message = dynuEx.Message, StatusCode = dynuEx.StatusCode, Type = dynuEx.Type };
            return JsonSerializer.SerializeToElement(apiError, GlobalJsonOptions.Options);
        }
        else if (ex.InnerException is not null && ex.InnerException is DynuApiException innerDynuEx)
        {
            var apiError = new ApiError() { Message = innerDynuEx.Message, StatusCode = innerDynuEx.StatusCode, Type = innerDynuEx.Type };
            return JsonSerializer.SerializeToElement(apiError, GlobalJsonOptions.Options);
        }
        else
        {
            return ex.Message;
        }
    }
}
