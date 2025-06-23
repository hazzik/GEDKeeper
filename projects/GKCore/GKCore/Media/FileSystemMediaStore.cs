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

        public override Stream MediaLoad(bool throwException)
        {
            string targetFn = BasePath + FileName;
            if (!File.Exists(targetFn)) {
                if (throwException) {
                    throw new MediaFileNotFoundException(targetFn);
                }

                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.MediaFileNotLoaded));
                return null;
            }

            return File.OpenRead(targetFn);
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
