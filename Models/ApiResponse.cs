using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Sakur.WebApiUtilities.Models
{
    public class ApiResponse : ObjectResult
    {
        /// <summary>
        /// Will create a new instance of an api response with a specified status code
        /// </summary>
        /// <param name="statusCode">The status code for the api response</param>
        public ApiResponse(HttpStatusCode statusCode = HttpStatusCode.OK) : base(statusCode)
        {
            StatusCode = (int)statusCode;
        }

        /// <summary>
        /// Will create a new instance of an api response with a specified message and status code. Will create an anonymous object with a message field
        /// </summary>
        /// <param name="message">The message to be included in the body of the http response</param>
        /// <param name="statusCode"><The status code for the api response/param>
        public ApiResponse(string message, HttpStatusCode statusCode = HttpStatusCode.OK) : base(statusCode)
        {
            Value = new { message };
            StatusCode = (int)statusCode;
        }

        /// <summary>
        /// Will create a new instance of an api response with an object for the response body and a statuscode
        /// </summary>
        /// <param name="value">The body of the http response</param>
        /// <param name="statusCode">The status code for the api response</param>
        public ApiResponse(object value, HttpStatusCode statusCode = HttpStatusCode.OK) : base(statusCode)
        {
            Value = value;
            StatusCode = (int)statusCode;
        }

        /// <summary>
        /// Will create a new instance of an api response from an exception
        /// </summary>
        /// <param name="exception">The exception to create the response from</param>
        public ApiResponse(ApiException exception) : base(exception)
        {
            if (exception.ErrorObject == null && exception.ErrorMessage != null)
                Value = new { message = exception.ErrorMessage };
            else if (exception.ErrorMessage == null && exception.ErrorObject != null)
                Value = exception.ErrorObject;
            else if (exception.ErrorMessage != null && exception.ErrorObject != null)
                Value = new { message = exception.ErrorMessage, exceptionData = exception.ErrorObject };

            StatusCode = (int)exception.StatusCode;
        }
    }
}
