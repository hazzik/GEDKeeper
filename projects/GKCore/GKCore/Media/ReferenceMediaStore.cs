using System;
using System.IO;
using BSLib;

namespace GKCore.Types
{
    public sealed class ReferenceMediaStore : FileSystemMediaStore
    {
        public ReferenceMediaStore(string fileName, bool allowDelete)
            : base(string.Empty, fileName, allowDelete)
        {
        }

        public override MediaStoreStatus VerifyMediaFile(out string fileName)
        {
            var result = MediaStoreStatus.mssBadData;
            try {
                fileName = FileName;

                if (!File.Exists(fileName)) {
                    string xFileName = FileHelper.NormalizeFilename(fileName);
                    if (!File.Exists(xFileName)) {
                        result = MediaStoreStatus.mssFileNotFound;
                    } else {
                        result = MediaStoreStatus.mssExists;
                        fileName = xFileName;
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

        protected override string NormalizeFileName(BaseContext baseContext)
        {
            return FileHelper.NormalizeFilename(FileName);
        }
    }
}
