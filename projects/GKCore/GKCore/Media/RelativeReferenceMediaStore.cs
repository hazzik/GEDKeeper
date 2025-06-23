using System;
using System.IO;
using System.Threading.Tasks;
using BSLib;
using GKCore.Options;

namespace GKCore.Types
{
    public sealed class RelativeReferenceMediaStore :FileSystemMediaStore
    {
        public RelativeReferenceMediaStore(BaseContext baseContext, string fileName, bool allowDelete)
            : base(BaseContext.GetTreePath(baseContext.FileName), fileName, allowDelete)
        {
        }

        public override MediaStoreStatus VerifyMediaFile(out string fileName)
        {
            MediaStoreStatus result = MediaStoreStatus.mssBadData;

            try {
                fileName = BasePath + FileName;
                if (!File.Exists(fileName)) {
                    var xFileName = FileHelper.NormalizeFilename(fileName);
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

        public override bool MediaSave(BaseContext baseContext, out string refPath)
        {
            refPath = string.Empty;

            // set paths and links
            var targetFile = baseContext.GetTreeRelativePath(FileName);
            refPath = GKData.GKStoreTypes[(int)MediaStoreType.mstRelativeReference].Sign + targetFile;

            refPath = FileHelper.NormalizeFilename(refPath);

            // verify existence
            bool alreadyExists = baseContext.MediaExists(refPath);
            if (alreadyExists) {
                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileWithSameNameAlreadyExists));
                return false;
            }

            return true;
        }
    }
}
