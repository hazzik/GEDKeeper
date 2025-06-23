using System;
using System.IO;
using BSLib;

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

        public override MediaStoreStatus VerifyMediaFile(out string fileName)
        {
            if (!string.IsNullOrEmpty(BasePath) && !Directory.Exists(BasePath)) {
                fileName = FileName;
                return MediaStoreStatus.mssStgNotFound;
            }

            var result = MediaStoreStatus.mssBadData;

            try {
                fileName = BasePath + FileName;
                if (!File.Exists(fileName)) {
                    var xFileName = FileHelper.NormalizeFilename(fileName);
                    if (!File.Exists(xFileName)) {
                        result = MediaStoreStatus.mssFileNotFound;
                    } else {
                        fileName = xFileName;
                        result = MediaStoreStatus.mssExists;
                    }
                } else {
                    result = MediaStoreStatus.mssExists;
                }
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.VerifyMediaFile()", ex);
                fileName = string.Empty;
            }

            return result;
        }
    }
}
