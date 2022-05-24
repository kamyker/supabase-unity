using System;
using System.Collections.Generic;
using Supabase.Gotrue.Attributes;

namespace Supabase.Gotrue
{
    public static class Constants
    {
        public static string GOTRUE_URL = "http://localhost:9999";
        public static string AUDIENCE = "";
        public static readonly Dictionary<string, string> DEFAULT_HEADERS = new Dictionary<string, string>();
        public static int EXPIRY_MARGIN = 60 * 1000;
        public static string STORAGE_KEY = "supabase.auth.token";
        public static readonly Dictionary<string, object> COOKIE_OPTIONS = new Dictionary<string, object>{
            { "name", "sb:token" },
            { "lifetime", 60 * 60 * 8 },
            { "domain", "" },
            { "path", "/" },
            { "sameSite", "lax" }
        };

        public enum SortOrder
        {
            [MapTo("asc")]
            Ascending,
            [MapTo("desc")]
            Descending
        }
    }
}
