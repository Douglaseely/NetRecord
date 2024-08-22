using System.Text.Json;
using NetRecord.Services;
using NetRecord.Utils.Exceptions;
using NetRecord.Utils.Serialization;

namespace NetRecord.Utils.Models;

/// <summary>
/// To have an instance of this class implies an existing json file that has been read from.
/// This class cannot exist without having read or created a json file, and as such is always assumed to be up to date
/// </summary>
internal class RecordFile
{
    private string _filePath;
    private string _fileName;
    public IEnumerable<NetRecordTransaction> Recordings;

    public RecordFile UpdateFileWithTransaction(
        NetRecordTransaction transaction,
        NetRecordConfiguration configuration,
        bool bypassCheck = false
    )
    {
        NetRecordTransaction? matchingRecord = null;
        if (!bypassCheck)
        {
            matchingRecord = Recordings.FirstOrDefault(t =>
                t.CheckIfMatch(transaction, configuration)
            );
        }

        if (matchingRecord is null)
            Recordings = Recordings.Append(transaction);
        else
        {
            var recordingList = Recordings.ToList();
            recordingList.Remove(matchingRecord);
            recordingList.Add(transaction);
            Recordings = recordingList;
        }

        Write(configuration);

        return this;
    }

    public static RecordFile GetorCreateRecordFile(
        NetRecordConfiguration configuration,
        NetRecordTransaction transaction
    )
    {
        var filePath = configuration.GetPathFromRoot();
        var fileName = configuration.GetFileName(transaction);
        var fullPath = Path.Join(filePath, fileName);

        return File.Exists(fullPath)
            ? ReadFromFile(filePath, fileName, configuration.JsonSerializerOptions)
            : new RecordFile(filePath, fileName);
    }

    private static RecordFile ReadFromFile(
        string filePath,
        string fileName,
        JsonSerializerOptions jsonSerializerOptions
    )
    {
        // TODO: This needs to be done for replay, passing on it now until I'm working on replay
        var fileText = File.ReadAllText(Path.Join(filePath, fileName));
        var recordings = JsonUtils.DeserializeNullableJsonToObject<
            IEnumerable<NetRecordTransaction>
        >(fileText, jsonSerializerOptions);

        return new RecordFile
        {
            _filePath = filePath,
            _fileName = fileName,
            Recordings = recordings
        };
    }

    private void Write(NetRecordConfiguration configuration)
    {
        var fullPath = Path.Join(_filePath, _fileName);
        var serializedFile = JsonUtils.SerializeObjectToJson(
            Recordings,
            configuration.JsonSerializerOptions
        );
        if (string.IsNullOrEmpty(serializedFile))
            throw new NetRecordException("Error while serializing request transaction");

        File.WriteAllText(fullPath, serializedFile);
        File.AppendAllText(fullPath, Environment.NewLine);
    }

    private RecordFile() { }

    private RecordFile(string filePath, string fileName)
    {
        var fullPath = Path.Join(filePath, fileName);
        using var newFile = File.Create(fullPath);

        _filePath = filePath;
        _fileName = fileName;
        Recordings = [];
    }
}
