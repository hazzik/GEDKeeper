using System;
using System.IO;
using System.Threading.Tasks;
using BSLib;
using GKCore.Options;

namespace GKCore.Types
{
    public sealed class ReferenceMediaStore : FileSystemMediaStore, IMediaStore
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

        public bool MediaSave(BaseContext baseContext, out string refPath)
        {
            // set paths and links
            refPath = FileHelper.NormalizeFilename(FileName);

            // verify existence
            if (baseContext.MediaExists(refPath)) {
                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileWithSameNameAlreadyExists));
                return false;
            }

            return true;
        }
    }
}
