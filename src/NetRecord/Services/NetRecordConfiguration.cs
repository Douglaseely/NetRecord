using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetRecord.Interfaces;
using NetRecord.Utils;
using NetRecord.Utils.Enums;
using NetRecord.Utils.Extensions;
using NetRecord.Utils.Models;

namespace NetRecord.Services;

public class NetRecordConfiguration : INetRecordConfiguration
{
    #region Required Settings
    
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
    public Expression<Func<NetRecordTransaction, object>>? FileGroupIdentifier { get; set; } = null;

    /// <summary>
    /// This value should be an expression body that returns a list of calls of the RequestMessage,
    /// that will be used to match any sent request with a request recording,
    /// the function will be run on both new requests and saved recordings and the values checked against eachother.
    /// By default, only the Method and URI will be used. 
    /// </summary>
    public Func<NetRecordTransaction, object?>[] UniqueIdentifiers { get; set; } = [
        request => request.Request.Method.Method,
        request => request.Request.Uri
    ];
    
    /// <summary>
    /// The censors to be applied to both the requests and responses saved, hiding sensitive data.
    /// </summary>
    public RequestCensors RequestCensors { get; set; } = new();

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
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// If set to true, a NetRecordFactory will only ever generate and return a single continuous HttpClient,
    /// rather than generating a new one on every create call.
    /// </summary>
    public bool FactoryReturnsSingleClient { get; set; } = false;

    #endregion

    public string GetFileName()
    {
        var baseName = RecordingName.Invoke();

        return baseName + FileGroupIdentifier?.GetPropertyInfo().Name + ".json";
    }

    public string GetPathFromRoot()
    {
        var rootPath = DirectoryUtils.GetRootPath();
        return Path.Join(rootPath, RecordingsDir.Invoke());
    }
}