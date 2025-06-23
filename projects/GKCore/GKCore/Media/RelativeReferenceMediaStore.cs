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

        protected override string NormalizeFileName(BaseContext baseContext)
        {
            var targetFile = baseContext.GetTreeRelativePath(FileName);
            var refPath = GKData.GKStoreTypes[(int)MediaStoreType.mstRelativeReference].Sign + targetFile;

            return FileHelper.NormalizeFilename(refPath);
        }
    }
}
