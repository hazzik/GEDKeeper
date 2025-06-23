using System;
using System.IO;
using System.Threading.Tasks;
using GKCore.Options;

namespace GKCore.Types
{
    public abstract class FileSystemMediaStore : MediaStore
    {
        protected readonly string BasePath;
        protected readonly string FileName;
        private readonly bool fAllowDelete;

        protected FileSystemMediaStore(string basePath, string fileName, bool allowDelete)
        {
            BasePath = basePath;
            FileName = fileName;
            fAllowDelete = allowDelete;
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

        public override string MediaLoad()
        {
            try {
                var storeStatus = VerifyMediaFile(out var fileName);
                return FileExists(storeStatus, fileName);
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.MediaLoad_fn()", ex);
                return string.Empty;
            }
        }

        private static string FileExists(MediaStoreStatus storeStatus, string fileName)
        {
            if (storeStatus == MediaStoreStatus.mssExists)
                return fileName;

            switch (storeStatus) {
                case MediaStoreStatus.mssFileNotFound:
                    AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileNotFound, fileName));
                    break;
                case MediaStoreStatus.mssStgNotFound:
                    AppHost.StdDialogs.ShowError(LangMan.LS(LSID.StgNotFound));
                    break;
                case MediaStoreStatus.mssArcNotFound:
                    AppHost.StdDialogs.ShowError(LangMan.LS(LSID.ArcNotFound));
                    break;
            }

            return string.Empty;
        }

        public override async Task<bool> MediaDelete()
        {
            if (!fAllowDelete) {
                return true;
            }

            try {
                var storeStatus = VerifyMediaFile(out var fileName);

                switch (storeStatus) {
                    case MediaStoreStatus.mssExists:
                        if (!await ConfirmDelete()) {
                            return false;
                        }

                        return DeleteCore(fileName);

                    case MediaStoreStatus.mssFileNotFound:
                        return await AppHost.StdDialogs.ShowQuestion(LangMan.LS(LSID.ContinueQuestion, LangMan.LS(LSID.FileNotFound, fileName)));

                    case MediaStoreStatus.mssStgNotFound:
                        return await AppHost.StdDialogs.ShowQuestion(LangMan.LS(LSID.ContinueQuestion, LangMan.LS(LSID.StgNotFound)));

                    case MediaStoreStatus.mssArcNotFound:
                        return await AppHost.StdDialogs.ShowQuestion(LangMan.LS(LSID.ContinueQuestion, LangMan.LS(LSID.ArcNotFound)));
                    case MediaStoreStatus.mssBadData:
                        return true;
                }

                return false;
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.MediaDelete()", ex);
                return false;
            }
        }

        protected virtual bool DeleteCore(string fileName)
        {
            File.Delete(fileName);
            return true;
        }

        private static async Task<bool> ConfirmDelete()
        {
            if (GlobalOptions.Instance.DeleteMediaFileWithoutConfirm) {
                return true;
            }

            // TODO: may be Yes/No/Cancel?
            return await AppHost.StdDialogs.ShowQuestion(LangMan.LS(LSID.MediaFileDeleteQuery));
        }
    }
}
