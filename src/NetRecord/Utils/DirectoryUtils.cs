namespace NetRecord.Utils;

public static class DirectoryUtils
{
    public static string GetRootPath(string fileName)
    {
        // First, try to get the application's base directory
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // For development environment (looking for .sln file)
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            if (Directory.GetFiles(dir, fileName).Length > 0)
                return dir;

            var parentDir = Directory.GetParent(dir);
            if (parentDir == null)
                break;

            dir = parentDir.FullName;
        }

        if (baseDirectory is null)
            throw new Exception("Could not find solution root nor active project root directory");

        // If no .sln file found, return the base directory
        return baseDirectory;
    }
}
