using System;
using System.IO;

namespace GKCore.Types
{
    public abstract class FileSystemMediaStore
    {
        protected readonly string BasePath;
        protected readonly string FileName;

        protected FileSystemMediaStore(string basePath, string fileName)
        {
            BasePath = basePath;
            FileName = fileName;
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
    }
}
