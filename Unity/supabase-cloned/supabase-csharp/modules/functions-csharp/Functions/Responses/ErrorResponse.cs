using System;
using Newtonsoft.Json;

namespace Supabase.Functions.Responses
{
    /// <summary>
    /// A representation of Postgrest's API error response.
    /// </summary>
    public class ErrorResponse : BaseResponse
    {
        public string Message { get; set; }
    }
}
