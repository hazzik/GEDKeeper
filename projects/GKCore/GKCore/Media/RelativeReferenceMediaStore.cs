using System;
using System.IO;
using System.Threading.Tasks;
using BSLib;
using GKCore.Options;

namespace GKCore.Types
{
    public sealed class RelativeReferenceMediaStore :FileSystemMediaStore,IMediaStore
    {
        public RelativeReferenceMediaStore(BaseContext baseContext, string fileName)
            : base(BaseContext.GetTreePath(baseContext.FileName), fileName)
        {
        }

        public async Task<bool> MediaDelete()
        {
            try {
                var storeStatus = VerifyMediaFile(out var fileName);
                var result = false;

                switch (storeStatus) {
                    case MediaStoreStatus.mssExists:
                        if (!GlobalOptions.Instance.AllowDeleteMediaFileFromRefs) {
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
            MediaStoreStatus result = MediaStoreStatus.mssBadData;

            try {
                fileName = BasePath + FileName;
                if (!File.Exists(fileName)) {
                    var xFileName = FileHelper.NormalizeFilename(fileName);
                    if (string.IsNullOrEmpty(xFileName)) {
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
