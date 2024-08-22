namespace NetRecord.Utils;

public static class DirectoryUtils
{
    public static string GetRootPath()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            if (Directory.GetFiles(dir, "*.sln").Length > 0)
                return dir;

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new Exception("Could not find solution root");
    }
}
