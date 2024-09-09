using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetRecord.Interfaces;
using NetRecord.Utils;
using NetRecord.Utils.Enums;
using NetRecord.Utils.Exceptions;
using NetRecord.Utils.Extensions;
using NetRecord.Utils.Models;

namespace NetRecord.Services;

public class NetRecordConfiguration : INetRecordConfiguration
{
    #region Required Settings

    /// <summary>
    /// Defines what HttpClient these configuration settings are associated with.
    /// Automatically set and handled within NetRecord when using its extension methods.
    /// </summary>
    public string ClientName { get; set; }

    /// <summary>
    /// The service mode will decide what actions the service takes when it attempts to make a request as follows:
    /// Auto: Try to find and use a recorded request if one exists, or make and record the request if it does not
    /// Record: Record the request into a RecordFile, overwriting any previously made requests with the same unique identifiers
    /// Replay: Replay all requests sent, erroring if a saved request does not exist
    /// Bypass: Act as a normal HttpClient, without recording or replaying requests
    /// </summary>
    public required ServiceMode Mode { get; set; }

    /// <summary>
    /// The path for the recordings to be saved too, this is a func in case the user wants to use logic to
    /// dynamically change the path depending on what assembly the request is running from.
    /// The path this returns should be a local filepath starting from the solution root
    /// </summary>
    public required Func<string> RecordingsDir { get; set; }

    #endregion

    #region Optional Settings

    /// <summary>
    /// This value will decide if every request recorded will be recorded into its own file,
    /// or if they will all be grouped into their own file. The expression body for this should return a
    /// property of the RequestMessage, which will be used to group similar requests.
    /// This value will additionally be added to the end of the recording file name.
    /// </summary>
    /// <exception cref="NetRecordException">If the expression does not contain exclusively a single property call of the HttpRequestMessage</exception>
    public Expression<Func<NetRecordTransaction, object>>? FileGroupIdentifier { get; set; }

    /// <summary>
    /// This value should be an expression body that returns a list of calls of the RequestMessage,
    /// that will be used to match any sent request with a request recording,
    /// the function will be run on both new requests and saved recordings and the values checked against eachother.
    /// By default, only the request Method, Body, and URI will be used.
    /// </summary>
    public Func<NetRecordRequest, object?>[] UniqueIdentifiers { get; set; } =
        [request => request.Method.Method, request => request.Uri, request => request.Body];

    /// <summary>
    /// The censors to be applied to both the requests and responses saved, hiding sensitive data.
    /// </summary>
    public RequestCensors RequestCensors { get; set; } = RequestCensors.DefaultSensitive;

    /// <summary>
    /// The name for the recording json file that will be saved to the file path,
    /// if a name is not defined then it will default to "NetRecordRecording".
    /// Note that if a uniqueFileIdentifier is defined, it will be added to the file name.
    /// Secondary note that the ".json" file extension will be added automatically.
    /// </summary>
    public Func<string> RecordingName { get; set; } = () => "NetRecordRecording";

    /// <summary>
    /// The jsonSerializerOptions that ALL serialization and deserialization will use
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } =
        new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

    #endregion

    #region Internal Utilties

    public string GetFileName(NetRecordTransaction transaction)
    {
        var baseName = RecordingName.Invoke();

        return baseName + GetFileNameExtension(transaction) + ".json";
    }

    public string GetPathFromRoot()
    {
        var rootPath = DirectoryUtils.GetRootPath();
        return Path.Join(rootPath, RecordingsDir.Invoke());
    }

    public string GetFileNameExtension(NetRecordTransaction transaction)
    {
        // If we aren't grouping the file, we don't need to add anything
        if (FileGroupIdentifier is null)
            return "";

        var fileExtensionString = "_" + FileGroupIdentifier.GetPropertyInfo().Name + "_";

        var groupingKey = FileGroupIdentifier?.Compile().Invoke(transaction).ToString();
        if (groupingKey is null)
            throw new NetRecordException("No value in file group identifier");

        if (groupingKey.Length > 32)
        {
            using var sha256 = SHA256.Create();
            var byteArray = sha256.ComputeHash(Encoding.UTF8.GetBytes(groupingKey));

            // Convert the byte array to a hexadecimal string
            var result = new StringBuilder();
            foreach (var item in byteArray)
            {
                result.Append(item.ToString("x2"));
            }

            var hashedKey = result.ToString()[..32];

            fileExtensionString += hashedKey;
        }
        else
        {
            fileExtensionString += groupingKey;
        }

        return fileExtensionString;
    }

    internal NetRecordConfiguration() { }

    #endregion

    /// <summary>
    /// Create your own NetRecordConfiguration
    /// </summary>
    /// <param name="mode">The RecordingMode that will be used to decide how requests are handled</param>
    /// <param name="recordingDir">The directory that will all recording files will be saved too</param>
    /// <param name="recordingName">The base name for the recording files saved.</param>
    /// <param name="fileGroupIdentifier">The identifier value that will be used to match requests into files</param>
    /// <param name="uniqueIdentifiers">The values that will be used to match two requests to each other for record rewriting and replaying</param>
    /// <param name="requestCensors">The censors that will hide and remove data from requests before saving</param>
    /// <returns>A newly created configuration</returns>
    public static NetRecordConfiguration Create(
        ServiceMode mode,
        Func<string> recordingDir,
        Func<string>? recordingName = null,
        Expression<Func<NetRecordTransaction, object>>? fileGroupIdentifier = null,
        Func<NetRecordRequest, object>[]? uniqueIdentifiers = null,
        RequestCensors? requestCensors = null
    )
    {
        var config = new NetRecordConfiguration { Mode = mode, RecordingsDir = recordingDir };

        if (fileGroupIdentifier is not null)
            config.FileGroupIdentifier = fileGroupIdentifier;

        if (uniqueIdentifiers is not null)
            config.UniqueIdentifiers = uniqueIdentifiers;

        if (requestCensors is not null)
            config.RequestCensors = requestCensors;

        if (recordingName is not null)
            config.RecordingName = recordingName;

        return config;
    }

    /// <summary>
    /// Create your own NetRecordConfiguration
    /// </summary>
    /// <param name="mode">The RecordingMode that will be used to decide how requests are handled</param>
    /// <param name="recordingDir">The directory that will all recording files will be saved too</param>
    /// <param name="recordingName">The base name for the recording files saved.</param>
    /// <param name="fileGroupIdentifier">The identifier value that will be used to match requests into files</param>
    /// <param name="uniqueIdentifiers">The values that will be used to match two requests to each other for record rewriting and replaying</param>
    /// <param name="requestCensors">The censors that will hide and remove data from requests before saving</param>
    /// <returns>A newly created configuration</returns>
    public static NetRecordConfiguration Create(
        ServiceMode mode,
        Func<string> recordingDir,
        string recordingName,
        Expression<Func<NetRecordTransaction, object>>? fileGroupIdentifier = null,
        Func<NetRecordRequest, object>[]? uniqueIdentifiers = null,
        RequestCensors? requestCensors = null
    )
    {
        var config = new NetRecordConfiguration { Mode = mode, RecordingsDir = recordingDir };

        if (fileGroupIdentifier is not null)
            config.FileGroupIdentifier = fileGroupIdentifier;

        if (uniqueIdentifiers is not null)
            config.UniqueIdentifiers = uniqueIdentifiers;

        if (requestCensors is not null)
            config.RequestCensors = requestCensors;

        if (recordingName is not null)
            config.RecordingName = () => recordingName;

        return config;
    }

    /// <summary>
    /// Create your own NetRecordConfiguration
    /// </summary>
    /// <param name="mode">The RecordingMode that will be used to decide how requests are handled</param>
    /// <param name="recordingDir">The directory that will all recording files will be saved too</param>
    /// <param name="recordingName">The base name for the recording files saved.</param>
    /// <param name="fileGroupIdentifier">The identifier value that will be used to match requests into files</param>
    /// <param name="uniqueIdentifiers">The values that will be used to match two requests to each other for record rewriting and replaying</param>
    /// <param name="requestCensors">The censors that will hide and remove data from requests before saving</param>
    /// <returns>A newly created configuration</returns>
    public static NetRecordConfiguration Create(
        ServiceMode mode,
        string recordingDir,
        Func<string>? recordingName = null,
        Expression<Func<NetRecordTransaction, object>>? fileGroupIdentifier = null,
        Func<NetRecordRequest, object>[]? uniqueIdentifiers = null,
        RequestCensors? requestCensors = null
    )
    {
        var config = new NetRecordConfiguration { Mode = mode, RecordingsDir = () => recordingDir };

        if (fileGroupIdentifier is not null)
            config.FileGroupIdentifier = fileGroupIdentifier;

        if (uniqueIdentifiers is not null)
            config.UniqueIdentifiers = uniqueIdentifiers;

        if (requestCensors is not null)
            config.RequestCensors = requestCensors;

        if (recordingName is not null)
            config.RecordingName = recordingName;

        return config;
    }

    /// <summary>
    /// Create your own NetRecordConfiguration
    /// </summary>
    /// <param name="mode">The RecordingMode that will be used to decide how requests are handled</param>
    /// <param name="recordingDir">The directory that will all recording files will be saved too</param>
    /// <param name="recordingName">The base name for the recording files saved.</param>
    /// <param name="fileGroupIdentifier">The identifier value that will be used to match requests into files</param>
    /// <param name="uniqueIdentifiers">The values that will be used to match two requests to each other for record rewriting and replaying</param>
    /// <param name="requestCensors">The censors that will hide and remove data from requests before saving</param>
    /// <returns>A newly created configuration</returns>
    public static NetRecordConfiguration Create(
        ServiceMode mode,
        string recordingDir,
        string recordingName,
        Expression<Func<NetRecordTransaction, object>>? fileGroupIdentifier = null,
        Func<NetRecordRequest, object>[]? uniqueIdentifiers = null,
        RequestCensors? requestCensors = null
    )
    {
        var config = new NetRecordConfiguration { Mode = mode, RecordingsDir = () => recordingDir };

        if (fileGroupIdentifier is not null)
            config.FileGroupIdentifier = fileGroupIdentifier;

        if (uniqueIdentifiers is not null)
            config.UniqueIdentifiers = uniqueIdentifiers;

        if (requestCensors is not null)
            config.RequestCensors = requestCensors;

        if (recordingName is not null)
            config.RecordingName = () => recordingName;

        return config;
    }
}
