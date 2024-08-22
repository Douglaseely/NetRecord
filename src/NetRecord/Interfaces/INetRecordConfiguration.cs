using System.Linq.Expressions;
using System.Text.Json;
using NetRecord.Utils.Enums;
using NetRecord.Utils.Exceptions;
using NetRecord.Utils.Models;

namespace NetRecord.Interfaces;

public interface INetRecordConfiguration
{
   #region Required Settings
    
    /// <summary>
    /// The service mode will decide what actions the service takes when it attempts to make a request as follows:
    /// Auto: Try to find and use a recorded request if one exists, or make and record the request if it does not
    /// Record: Record the request into a RecordFile, overwriting any previously made requests with the same unique identifiers
    /// Replay: Replay all requests sent, erroring if a saved request does not exist
    /// Bypass: Act as a normal HttpClient, without recording or replaying requests
    /// </summary>
    public ServiceMode Mode { get; set; }
    
    /// <summary>
    /// The path for the recordings to be saved too, this is a func in case the user wants to use logic to
    /// dynamically change the path depending on what assembly the request is running from.
    /// The path this returns should be a local filepath starting from the solution root
    /// </summary>
    public Func<string> RecordingsDir { get; set; }
    
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
    /// By default, only the Method and URI will be used. 
    /// </summary>
    public Func<NetRecordRequest, object?>[] UniqueIdentifiers { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public RequestCensors RequestCensors { get; set; }

    /// <summary>
    /// The name for the recording json file that will be saved to the file path,
    /// if a name is not defined then it will default to "NetRecordRecording".
    /// Note that if a uniqueFileIdentifier is defined, it will be added to the file name.
    /// Secondary note that the ".json" file extension will be added automatically.
    /// </summary>
    public Func<string> RecordingName { get; set; }

    
    public JsonSerializerOptions JsonSerializerOptions { get; set; }

    /// <summary>
    /// If set to true, a NetRecordFactory will only ever generate and return a single continuous HttpClient,
    /// rather than generating a new one on every create call.
    /// </summary>
    public bool FactoryReturnsSingleClient { get; set; }

    #endregion

    internal string GetFileName(NetRecordTransaction transaction);

    internal string GetPathFromRoot();
}