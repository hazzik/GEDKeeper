using System;
using System.IO;
using BSLib;
using GDModel;

namespace GKCore.Types
{
    public sealed class StorageMediaStore : FileSystemMediaStore
    {
        public StorageMediaStore(BaseContext baseContext, string fileName, bool allowDelete) :
            base(baseContext.GetStgFolder(), fileName, allowDelete)
        {
        }

        public override MediaStoreStatus VerifyMediaFile(out string fileName)
        {
            var result = MediaStoreStatus.mssBadData;

            try {
                fileName = FileName;

                if (!Directory.Exists(BasePath)) {
                    result = MediaStoreStatus.mssStgNotFound;
                } else {
                    fileName = BasePath + fileName;
                    if (!File.Exists(fileName)) {
                        result = MediaStoreStatus.mssFileNotFound;
                    } else {
                        result = MediaStoreStatus.mssExists;
                    }
                }
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.VerifyMediaFile()", ex);
                fileName = string.Empty;
                return result;
            }

            return result;
        }

        public override bool MediaSave(BaseContext baseContext, out string refPath)
        {
            string storeFile = Path.GetFileName(FileName);
            string storePath = GKUtils.GetStoreFolder(GKUtils.GetMultimediaKind(GDMFileReference.RecognizeFormat(FileName)));

            refPath = string.Empty;

            // set paths and links
            var targetFile = storePath + storeFile;
            refPath = GKData.GKStoreTypes[(int)MediaStoreType.mstStorage].Sign + targetFile;

            refPath = FileHelper.NormalizeFilename(refPath);

            // verify existence
            bool alreadyExists = baseContext.MediaExists(refPath);
            if (alreadyExists) {
                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileWithSameNameAlreadyExists));
                return false;
            }

            bool result;

            // save a copy to archive or storage
            string targetFn = string.Empty;
            try {
                string targetDir = BasePath + storePath;
                if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                targetFn = targetDir + storeFile;
                result = baseContext.CopyFile(FileName, targetFn, !AppHost.TEST_MODE);
            } catch (IOException ex) {
                Logger.WriteError(string.Format("BaseContext.MediaSave({0}, {1})", FileName, targetFn), ex);
                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileWithSameNameAlreadyExists));
                result = false;
            }

            return result;
        }
    }
}
