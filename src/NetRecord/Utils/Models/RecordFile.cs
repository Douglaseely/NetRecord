using NetRecord.Services;
using NetRecord.Utils.Extensions;

namespace NetRecord.Utils.Models;

/// <summary>
/// To have an instance of this class implies an existing json file that has been read from.
/// This class cannot exist without having read or created a json file, and as such is always assumed to be up to date
/// </summary>
internal class RecordFile
{
    private string _filePath;
    private string _fileName;
    public NetRecordTransaction[] Recordings;

    public RecordFile UpdateFileWithTransaction(NetRecordTransaction transaction, NetRecordConfiguration configuration)
    {
        var mathingRecord = Recordings.FirstOrDefault(t => t.CheckIfMatch(transaction, configuration));
    }

    public static RecordFile GetorCreateRecordFile(NetRecordConfiguration configuration)
    {
        var filePath = configuration.GetPathFromRoot();
        var fileName = configuration.GetFileName();
        var fullPath = Path.Join(filePath, fileName);

        return File.Exists(fullPath) 
            ? ReadFromFile(filePath, fileName) 
            : new RecordFile(filePath, fileName);
    }

    private static RecordFile ReadFromFile(string filePath, string fileName)
    {
        // TODO: This needs to be done for replay, passing on it now until I'm working on replay
        var fileText = File.ReadAllText(Path.Join(filePath, fileName));
        var recordings = Array.Empty<NetRecordTransaction>();

        return new RecordFile
        {
            _filePath = filePath,
            _fileName = fileName,
            Recordings = recordings
        };
    }
    
    private RecordFile()
    {
        
    }

    private RecordFile(string filePath, string fileName)
    {
        var fullPath = Path.Join(filePath, fileName);
        using var newFile = File.Create(fullPath);

        _filePath = filePath;
        _fileName = fileName;
        Recordings = [];
    }
}