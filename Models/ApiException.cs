﻿using System;
using System.Net;

namespace Sakur.WebApiUtilities.Models
{
    /// <summary>
    /// An exception in the web api
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// String message containing a description of the error
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Status code to return from the api
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Object containing data for the exception
        /// </summary>
        public object? ErrorObject { get; private set; }

        /// <summary>
        /// Will create a new instance of an api exception with a message and a statuscode
        /// </summary>
        /// <param name="errorMessage">The message for the exception</param>
        /// <param name="statusCode">The status code for the exception</param>
        public ApiException(string errorMessage, HttpStatusCode statusCode) : base(errorMessage)
        {
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Will create a new instance of an api exception with an error object and a statuscode
        /// </summary>
        /// <param name="errorObject">Object containing data or information about the exception</param>
        /// <param name="statusCode">The status code for the exception</param>
        public ApiException(object errorObject, HttpStatusCode statusCode) : base(errorObject.ToString())
        {
            ErrorObject = errorObject;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Will create a new instance of an api exception with an error object, message and a status code
        /// </summary>
        /// <param name="errorObject">Object containing data or information about the exception</param>
        /// <param name="errorMessage">The message for the exception</param>
        /// <param name="statusCode">The status code for the exceptio</param>
        public ApiException(object errorObject, string errorMessage, HttpStatusCode statusCode) : base(errorMessage)
        {
            ErrorObject = errorObject;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Will convert the api exception to a readable string with the error message and the stack trace
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{ErrorMessage} ({StackTrace?.Trim()})";
        }
    }
}
