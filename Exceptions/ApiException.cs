﻿using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorDetails { get; }

        public ApiException(string message, HttpStatusCode statusCode, string details = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorDetails = details ?? message;
        }

        public ApiException(string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorDetails = message;
        }

        public override string ToString() =>
            $"[{(int)StatusCode} {StatusCode}] {Message}\nDetails: {ErrorDetails}";
    }

    public class ProblemDetails
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public int Status { get; set; }

    }
}
