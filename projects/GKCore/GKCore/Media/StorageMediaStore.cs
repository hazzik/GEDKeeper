using System;
using System.IO;
using BSLib;
using GDModel;
using GKCore.Interfaces;

namespace GKCore.Types
{
    public sealed class StorageMediaStore : FileSystemMediaStore
    {
        public StorageMediaStore(IBaseContext baseContext, string fileName, bool allowDelete) :
            base(baseContext.GetStgFolder(), fileName, allowDelete)
        {
        }

        protected override bool SaveCopy(IBaseContext baseContext, string targetFile)
        {
            // save a copy to storage
            var targetFn = BasePath + targetFile;
            try {
                return baseContext.CopyFile(FileName, targetFn, !AppHost.TEST_MODE);
            } catch (IOException ex) {
                Logger.WriteError(string.Format("BaseContext.MediaSave({0}, {1})", FileName, targetFn), ex);
                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileWithSameNameAlreadyExists));
                return false;
            }
        }

        protected override string NormalizeFileName(IBaseContext baseContext)
        {
            var storeFile = Path.GetFileName(FileName);
            var storePath = GKUtils.GetStoreFolder(GKUtils.GetMultimediaKind(GDMFileReference.RecognizeFormat(FileName)));
            var targetDir = BasePath + storePath;
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            return FileHelper.NormalizeFilename(storePath + storeFile);
        }

        protected override string CreateRefPath(string targetFile)
        {
            return GKData.GKStoreTypes[(int)MediaStoreType.mstStorage].Sign + targetFile;
        }
    }
}
