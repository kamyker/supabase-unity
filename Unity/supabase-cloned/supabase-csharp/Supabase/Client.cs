﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Postgrest;
using Postgrest.Models;
using Postgrest.Responses;
using Supabase.Gotrue;
using static Supabase.Functions.Client;

namespace Supabase
{
    /// <summary>
    /// A singleton class representing a Supabase Client.
    /// </summary>
    public class Client
    {
        public enum ChannelEventType
        {
            Insert,
            Update,
            Delete,
            All
        }

        /// <summary>
        /// Supabase Auth allows you to create and manage user sessions for access to data that is secured by access policies.
        /// </summary>
        public Gotrue.Client Auth { get; private set; }
        public Realtime.Client Realtime { get; private set; }

        /// <summary>
        /// Supabase Edge functions allow you to deploy and invoke edge functions.
        /// </summary>
        public SupabaseFunctions Functions => new SupabaseFunctions(instance.FunctionsUrl, instance.GetAuthHeaders());

        private Postgrest.Client Postgrest() => global::Postgrest.Client.Initialize(instance.RestUrl, new Postgrest.ClientOptions
        {
            Headers = instance.GetAuthHeaders(),
            Schema = Schema
        });

        private static Client instance;
        public static Client Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.WriteLine("Supabase must be initialized before it is called.");
                    return null;
                }
                return instance;
            }
        }

        public string SupabaseKey { get; private set; }
        public string SupabaseUrl { get; private set; }
        public string AuthUrl { get; private set; }
        public string RestUrl { get; private set; }
        public string RealtimeUrl { get; private set; }
        public string StorageUrl { get; private set; }
        public string FunctionsUrl { get; private set; }
        public string Schema { get; private set; }

        private SupabaseOptions options;

        private Client() { }


        /// <summary>
        /// Initializes a Supabase Client.
        /// </summary>
        /// <param name="supabaseUrl"></param>
        /// <param name="supabaseKey"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static void Initialize(string supabaseUrl, string supabaseKey, SupabaseOptions options = null, Action<Client> callback = null)
        {
            Task.Run(async () =>
            {
                var result = await InitializeAsync(supabaseUrl, supabaseKey, options);
                callback?.Invoke(result);
            });
        }

        /// <summary>
        /// Initializes a Supabase Client Asynchronously.
        /// </summary>
        /// <param name="supabaseUrl"></param>
        /// <param name="supabaseKey"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<Client> InitializeAsync(string supabaseUrl, string supabaseKey, SupabaseOptions options = null)
        {
            instance = new Client();

            instance.SupabaseUrl = supabaseUrl;
            instance.SupabaseKey = supabaseKey;

            if (options == null)
                options = new SupabaseOptions();

            instance.options = options;
            instance.AuthUrl = string.Format(options.AuthUrlFormat, supabaseUrl);
            instance.RestUrl = string.Format(options.RestUrlFormat, supabaseUrl);
            instance.RealtimeUrl = string.Format(options.RealtimeUrlFormat, supabaseUrl).Replace("http", "ws");
            instance.StorageUrl = string.Format(options.StorageUrlFormat, supabaseUrl);
            instance.Schema = options.Schema;

            // See: https://github.com/supabase/supabase-js/blob/09065a65f171bc28a9fd7b831af2c24e5f1a380b/src/SupabaseClient.ts#L77-L83
            var isPlatform = new Regex(@"(supabase\.co)|(supabase\.in)").Match(supabaseUrl);

            if (isPlatform.Success)
            {
                var parts = supabaseUrl.Split('.');
                instance.FunctionsUrl = $"{parts[0]}.functions.{parts[1]}.{parts[2]}";
            }
            else
            {
                instance.FunctionsUrl = string.Format(options.FunctionsUrlFormat, supabaseUrl);
            }

            // Init Auth
            instance.Auth = await Gotrue.Client.InitializeAsync(new Gotrue.ClientOptions
            {
                Url = instance.AuthUrl,
                Headers = instance.GetAuthHeaders(),
                AutoRefreshToken = options.AutoRefreshToken,
                PersistSession = options.PersistSession,
                SessionDestroyer = options.SessionDestroyer,
                SessionPersistor = options.SessionPersistor,
                SessionRetriever = options.SessionRetriever
            });
            instance.Auth.StateChanged += Auth_StateChanged;

            // Init Realtime
            if (options.ShouldInitializeRealtime)
            {
                instance.Realtime = Supabase.Realtime.Client.Initialize(instance.RealtimeUrl, new Realtime.ClientOptions
                {
                    Parameters = { ApiKey = instance.SupabaseKey }
                });

                if (options.AutoConnectRealtime)
                {
                    await instance.Realtime.ConnectAsync();
                }
            }

            return instance;
        }

        private static void Auth_StateChanged(object sender, ClientStateChanged e)
        {
            switch (e.State)
            {
                // Pass new Auth down to Realtime
                // Ref: https://github.com/supabase-community/supabase-csharp/issues/12
                case Gotrue.Client.AuthState.SignedIn:
                case Gotrue.Client.AuthState.TokenRefreshed:
                    if (Instance.Realtime != null)
                    {
                        Instance.Realtime.SetAuth(Instance.Auth.CurrentSession.AccessToken);
                    }
                    break;

                // Remove Realtime Subscriptions on Auth Signout.
                case Gotrue.Client.AuthState.SignedOut:
                    if (Instance.Realtime != null)
                    {
                        foreach (var subscription in Instance.Realtime.Subscriptions.Values)
                            subscription.Unsubscribe();

                        Instance.Realtime.Disconnect();
                    }
                    break;
            }
        }

        /// <summary>
        /// Supabase Storage allows you to manage user-generated content, such as photos or videos.
        /// </summary>
        public Storage.Client Storage => new Storage.Client(StorageUrl, GetAuthHeaders());

        /// <summary>
        /// Gets the Postgrest client to prepare for a query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SupabaseTable<T> From<T>() where T : BaseModel, new() => new SupabaseTable<T>();

        /// <summary>
        /// Runs a remote procedure.
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task<BaseResponse> Rpc(string procedureName, Dictionary<string, object> parameters) => Postgrest().Rpc(procedureName, parameters);


        internal Dictionary<string, string> GetAuthHeaders()
        {
            var headers = new Dictionary<string, string>();
            headers["apiKey"] = SupabaseKey;
            headers["X-Client-Info"] = Util.GetAssemblyVersion();

            // In Regard To: https://github.com/supabase/supabase-csharp/issues/5
            if (options.Headers.ContainsKey("Authorization"))
            {
                headers["Authorization"] = options.Headers["Authorization"];
            }
            else
            {
                var bearer = Auth?.CurrentSession?.AccessToken != null ? Auth.CurrentSession.AccessToken : SupabaseKey;
                headers["Authorization"] = $"Bearer {bearer}";
            }

            return headers;
        }
    }

    /// <summary>
    /// Options available for Supabase Client Configuration
    /// </summary>
    public class SupabaseOptions
    {
        public string Schema = "public";

        /// <summary>
        /// Should the Client automatically handle refreshing the User's Token?
        /// </summary>
        public bool AutoRefreshToken { get; set; } = true;

        /// <summary>
        /// Should the Client Initialize Realtime?
        /// </summary>
        public bool ShouldInitializeRealtime { get; set; } = false;

        /// <summary>
        /// Should the Client automatically connect to Realtime?
        /// </summary>
        public bool AutoConnectRealtime { get; set; } = false;

        /// <summary>
        /// Should the Client call <see cref="SessionPersistor"/>, <see cref="SessionRetriever"/>, and <see cref="SessionDestroyer"/>?
        /// </summary>
        public bool PersistSession { get; set; } = true;

        /// <summary>
        /// Function called to persist the session (probably on a filesystem or cookie)
        /// </summary>
        public Func<Session, Task<bool>> SessionPersistor = (Session session) => Task.FromResult<bool>(true);

        /// <summary>
        /// Function to retrieve a session (probably from the filesystem or cookie)
        /// </summary>
        public Func<Task<Session>> SessionRetriever = () => Task.FromResult<Session>(null);

        /// <summary>
        /// Function to destroy a session.
        /// </summary>
        public Func<Task<bool>> SessionDestroyer = () => Task.FromResult<bool>(true);

        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        public string AuthUrlFormat { get; set; } = "{0}/auth/v1";
        public string RestUrlFormat { get; set; } = "{0}/rest/v1";
        public string RealtimeUrlFormat { get; set; } = "{0}/realtime/v1";
        public string StorageUrlFormat { get; set; } = "{0}/storage/v1";

        public string FunctionsUrlFormat { get; set; } = "{0}/functions/v1";
    }
}
