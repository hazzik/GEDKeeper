using System;
using System.IO;
using System.Threading.Tasks;
using BSLib;
using GDModel;
using GKCore.Options;

namespace GKCore.Types
{
    public sealed class StorageMediaStore : FileSystemMediaStore, IMediaStore
    {
        private readonly bool fAllowDelete;

        public StorageMediaStore(BaseContext baseContext, string fileName, bool allowDelete) :
            base(baseContext.GetStgFolder(), fileName)
        {
            fAllowDelete = allowDelete;
        }

        public async Task<bool> MediaDelete()
        {
            try {
                var storeStatus = VerifyMediaFile(out var fileName);
                var result = false;

                switch (storeStatus) {
                    case MediaStoreStatus.mssExists:
                        if (!fAllowDelete) {
                            return true;
                        }

                        if (!GlobalOptions.Instance.DeleteMediaFileWithoutConfirm) {
                            string msg = string.Format(LangMan.LS(LSID.MediaFileDeleteQuery));
                            // TODO: may be Yes/No/Cancel?
                            var res = await AppHost.StdDialogs.ShowQuestion(msg);
                            if (!res) {
                                return false;
                            }
                        }

                        File.Delete(fileName);
                        result = true;
                        break;

                    case MediaStoreStatus.mssFileNotFound:
                        result = await AppHost.StdDialogs.ShowQuestion(LangMan.LS(LSID.ContinueQuestion, LangMan.LS(LSID.FileNotFound, fileName)));
                        break;

                    case MediaStoreStatus.mssStgNotFound:
                        result = await AppHost.StdDialogs.ShowQuestion(LangMan.LS(LSID.ContinueQuestion, LangMan.LS(LSID.StgNotFound)));
                        break;

                    case MediaStoreStatus.mssArcNotFound:
                        result = await AppHost.StdDialogs.ShowQuestion(LangMan.LS(LSID.ContinueQuestion, LangMan.LS(LSID.ArcNotFound)));
                        break;

                    case MediaStoreStatus.mssBadData:
                        // can be deleted
                        result = true;
                        break;
                }

                return result;
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.MediaDelete()", ex);
                return false;
            }
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

        public bool MediaSave(BaseContext baseContext, out string refPath)
        {
            string storeFile = Path.GetFileName(FileName);
            string storePath = GKUtils.GetStoreFolder(GKUtils.GetMultimediaKind(GDMFileReference.RecognizeFormat(FileName)));

            refPath = string.Empty;
            string targetFile = string.Empty;

            // set paths and links
            targetFile = storePath + storeFile;
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
