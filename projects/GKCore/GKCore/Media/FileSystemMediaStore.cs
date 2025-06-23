using System;
using System.IO;
using System.Threading.Tasks;
using GKCore.Options;

namespace GKCore.Types
{
    public abstract class FileSystemMediaStore
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

        public Stream MediaLoad(bool throwException)
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

        public string MediaLoad()
        {
            try {
                var storeStatus = VerifyMediaFile(out var fileName);
                return FileExists(storeStatus, fileName);
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.MediaLoad_fn()", ex);
                return string.Empty;
            }
        }


        public abstract MediaStoreStatus VerifyMediaFile(out string fileName);

        private static string FileExists(MediaStoreStatus storeStatus, string fileName)
        {
            switch (storeStatus) {
                case MediaStoreStatus.mssExists:
                    return fileName;
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
    }
}
