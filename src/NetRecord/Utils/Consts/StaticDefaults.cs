namespace NetRecord.Utils.Consts;

internal static class StaticDefaults
{
    internal const string RecordingHeaderKey = "X-Via-NetRecord-Recording";

    /// <summary>
    ///     Default headers to add to replayed requests.
    /// </summary>
    internal static readonly Dictionary<string, object> ReplayHeaders = new Dictionary<string, object>
    {
        { RecordingHeaderKey, "true" }
    };

    /// <summary>
    ///     Default list of headers to censor in the cassettes.
    /// </summary>
    internal static List<string> CredentialHeadersToHide => new List<string>
    {
        "authorization",
        "x-api-key"
    };

    /// <summary>
    ///     Default list of parameters to censor in the cassettes.
    /// </summary>
    internal static List<string> CredentialParametersToHide => new List<string>
    {
        "access_token",
        "apiKey",
        "apiToken",
        "api_key",
        "api_token",
        "api-key",
        "api-token",
        "client_id",
        "client_secret",
        "clientId",
        "clientSecret",
        "client-id",
        "client-secret",
        "key",
        "password",
        "secret",
        "token",
        "username",
    };
}