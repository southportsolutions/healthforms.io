using System;
using System.Collections.Generic;
using System.Text;
using HealthForms.Api.Core.Models.Errors;

namespace HealthForms.Api.Shared
{
    public class HealthFormsApiResponse<T> : HealthFormsApiResponse where T : class?
    {
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuccess => StatusCode is >= 200 and < 300;
        public int StatusCode { get; set; }
    }
    public class HealthFormsApiResponse
    {
        public HealthFormsErrorResponse? Error { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuccess => StatusCode is >= 200 and < 300;
        public int StatusCode { get; set; }
    }
}
