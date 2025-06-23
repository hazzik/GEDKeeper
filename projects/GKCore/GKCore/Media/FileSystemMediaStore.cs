using System.IO;

namespace GKCore.Types
{
    public abstract class FileSystemMediaStore : MediaStore
    {
        protected readonly string BasePath;
        protected readonly string FileName;

        protected FileSystemMediaStore(string basePath, string fileName, bool allowDelete) : base(allowDelete)
        {
            BasePath = basePath;
            FileName = fileName;
        }

        protected override Stream LoadStreamCore(string fileName)
        {
            return File.OpenRead(fileName);
        }

        protected override string LoadFileCore(string fileName)
        {
            return fileName;
        }

        protected override bool DeleteCore(string fileName)
        {
            File.Delete(fileName);
            return true;
        }
    }
}
