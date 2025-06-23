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

        protected FileSystemMediaStore(string basePath, string fileName, bool allowDelete) : base(allowDelete)
        {
            BasePath = basePath;
            FileName = fileName;
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

        protected override bool DeleteCore(string fileName)
        {
            File.Delete(fileName);
            return true;
        }
    }
}
