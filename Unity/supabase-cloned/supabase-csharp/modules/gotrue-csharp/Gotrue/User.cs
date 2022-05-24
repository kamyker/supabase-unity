using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#nullable enable
namespace Supabase.Gotrue
{
    /// <summary>
    /// Represents a Gotrue User
    /// Ref: https://supabase.github.io/gotrue-js/interfaces/User.html
    /// </summary>
    public class User
    {
        [JsonProperty("action_link")]
        public string? ActionLink { get; set; }

        [JsonProperty("app_metadata")]
        public Dictionary<string, object> AppMetadata { get; set; } = new Dictionary<string, object>();

        [JsonProperty("aud")]
        public string Aud { get; set; }

        [JsonProperty("confirmation_sent_at")]
        public DateTime? ConfirmationSentAt { get; set; }

        [JsonProperty("confirmed_at")]
        public DateTime? ConfirmedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("email_confirmed_at")]
        public DateTime? EmailConfirmedAt { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("identities")]
        public List<UserIdentity> Identities { get; set; } = new List<UserIdentity>();

        [JsonProperty("invited_at")]
        public DateTime? InvitedAt { get; set; }

        [JsonProperty("last_sign_in_at")]
        public DateTime? LastSignInAt { get; set; }

        [JsonProperty("phone")]
        public string? Phone { get; set; }

        [JsonProperty("phone_confirmed_at")]
        public DateTime? PhoneConfirmedAt { get; set; }

        [JsonProperty("recovery_sent_at")]
        public DateTime? RecoverySentAt { get; set; }

        [JsonProperty("role")]
        public string? Role { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("user_metadata")]
        public Dictionary<string, object> UserMetadata { get; set; } = new Dictionary<string, object>();

    }

    /// <summary>
    /// Ref: https://supabase.github.io/gotrue-js/interfaces/AdminUserAttributes.html
    /// </summary>
    public class AdminUserAttributes : UserAttributes
    {
        /// <summary>
        /// A custom data object for app_metadata that. Can be any JSON serializable data.
        /// Only a service role can modify
        ///
        /// Note: GoTrue does not yest support creating a user with app metadata
        ///     (see: https://github.com/supabase/gotrue-js/blob/d7b334a4283027c65814aa81715ffead262f0bfa/test/GoTrueApi.test.ts#L45)
        /// </summary>
        [JsonProperty("app_metadata")]
        public Dictionary<string, object> AppMetadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A custom data object for user_metadata. Can be any JSON serializable data.
        /// Only a service role can modify.
        /// </summary>
        [JsonProperty("user_metadata")]
        public Dictionary<string, object> UserMetadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Sets if a user has confirmed their email address.
        /// Only a service role can modify
        /// </summary>
        [JsonProperty("email_confirm")]
        public bool? EmailConfirm { get; set; }

        /// <summary>
        /// Sets if a user has confirmed their phone number.
        /// Only a service role can modify
        /// </summary>
        [JsonProperty("phone_confirm")]
        public bool? PhoneConfirm { get; set; }
    }

    /// <summary>
    /// Ref: https://supabase.github.io/gotrue-js/interfaces/UserAttributes.html
    /// </summary>
    public class UserAttributes
    {
        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("email_change_token")]
        public string? EmailChangeToken { get; set; }

        [JsonProperty("password")]
        public string? Password { get; set; }

        [JsonProperty("phone")]
        public string? Phone { get; set; }

        /// <summary>
        /// A custom data object for user_metadata that a user can modify.Can be any JSON.
        /// </summary>
        [JsonProperty("data")]
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Ref: https://supabase.github.io/gotrue-js/interfaces/VerifyEmailOTPParams.html
    /// </summary>
    public class VerifyOTPParams
    {
        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("phone")]
        public string? Phone { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class UserList
    {
        [JsonProperty("aud")]
        public string Aud { get; set; }

        [JsonProperty("users")]
        public List<User> Users { get; set; } = new List<User>();
    }

    /// <summary>
    /// Ref: https://supabase.github.io/gotrue-js/interfaces/UserIdentity.html
    /// </summary>
    public class UserIdentity
    {
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("identity_data")]
        public Dictionary<string, object> IdentityData { get; set; } = new Dictionary<string, object>();

        [JsonProperty("last_sign_in_at")]
        public DateTime LastSignInAt { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }
}
