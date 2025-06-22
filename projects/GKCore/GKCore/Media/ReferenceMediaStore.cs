using System;
using System.IO;
using System.Threading.Tasks;
using BSLib;
using GKCore.Options;

namespace GKCore.Types
{
    public sealed class ReferenceMediaStore : FileSystemMediaStore, IMediaStore
    {
        public ReferenceMediaStore(string fileName) : base(string.Empty, fileName)
        {
        }

        public async Task<bool> MediaDelete()
        {
            try {
                string fileName = FileName;


                MediaStoreStatus storeStatus = VerifyMediaFile(out fileName);
                bool result = false;

                switch (storeStatus) {
                    case MediaStoreStatus.mssExists: {
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

                        switch (MediaStoreType.mstReference)
                        {
                            default:
                                File.Delete(fileName);
                                break;
                        }
                        result = true;
                    }
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

                if (!File.Exists(fileName)) {
                    string xFileName = FileHelper.NormalizeFilename(fileName);
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
            // set paths and links
            refPath = FileHelper.NormalizeFilename(FileName);

            // verify existence
            if (baseContext.MediaExists(refPath)) {
                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileWithSameNameAlreadyExists));
                return false;
            }

            return true;
        }
    }
}
