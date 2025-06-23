using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using BSLib;
using GDModel;

namespace GKCore.Types
{
    public sealed class ArchiveMediaStore : MediaStore
    {
        private readonly string fArcFileName;
        private readonly string fFileName;

        public ArchiveMediaStore(BaseContext baseContext, string fileName, bool allowDelete) : base(allowDelete)
        {
            fFileName = fileName;
            fArcFileName = baseContext.GetArcFileName();
        }

        public override Stream MediaLoad(bool throwException)
        {
            Stream stream = new MemoryStream();
            if (!File.Exists(fArcFileName)) {
                if (throwException) {
                    throw new MediaFileNotFoundException(fArcFileName);
                }

                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.MediaFileNotLoaded));
            } else {
                ArcFileLoad(fFileName, stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            return stream;
        }

        public override string MediaLoad()
        {
            string fileName;
            try {
                bool ret;
                MediaStoreStatus storeStatus = VerifyMediaFile(out var fileName1);
                if (storeStatus != MediaStoreStatus.mssExists)
                {
                    switch (storeStatus) {
                        case MediaStoreStatus.mssFileNotFound:
                            AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileNotFound, fileName1));
                            break;

                        case MediaStoreStatus.mssStgNotFound:
                            AppHost.StdDialogs.ShowError(LangMan.LS(LSID.StgNotFound));
                            break;

                        case MediaStoreStatus.mssArcNotFound:
                            AppHost.StdDialogs.ShowError(LangMan.LS(LSID.ArcNotFound));
                            break;

                        case MediaStoreStatus.mssBadData:
                            break;
                    }

                    ret = false;
                }
                else
                {
                    ret = true;
                }

                if (!ret) {
                    return string.Empty;
                }

                if (!File.Exists(fArcFileName)) {
                    AppHost.StdDialogs.ShowError(LangMan.LS(LSID.MediaFileNotLoaded));
                    return string.Empty;
                }

                fileName = GKUtils.GetTempDir() + Path.GetFileName(fFileName);
                var targetFn = FileHelper.NormalizeFilename(fFileName);
                ExtractToFile(fileName, targetFn);
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.MediaLoad_fn()", ex);
                return string.Empty;
            }

            return fileName;
        }

        protected override bool DeleteCore(string fileName)
        {
            ArcFileDelete(fileName);
            return true;
        }

        public override MediaStoreStatus VerifyMediaFile(out string fileName)
        {
            MediaStoreStatus result = MediaStoreStatus.mssBadData;

            try {
                fileName = fFileName;

                if (!File.Exists(fArcFileName)) {
                    result = MediaStoreStatus.mssArcNotFound;
                } else {
                    if (!ArcFileExists(fileName)) {
                        result = MediaStoreStatus.mssFileNotFound;
                    } else {
                        result = MediaStoreStatus.mssExists;
                    }
                }
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.VerifyMediaFile()", ex);
                fileName = string.Empty;
            }

            return result;
        }

        public override bool MediaSave(BaseContext baseContext, out string refPath)
        {
            refPath = NormalizeFilename(out var targetFile);

            // verify existence
            bool alreadyExists = baseContext.MediaExists(refPath);
            if (alreadyExists) {
                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileWithSameNameAlreadyExists));
                return false;
            }

            // save a copy to archive
            ArcFileSave(fFileName, targetFile);

            return true;
        }

        private string NormalizeFilename(out string targetFile)
        {
            string storeFile = Path.GetFileName(fFileName);
            string storePath =
                GKUtils.GetStoreFolder(GKUtils.GetMultimediaKind(GDMFileReference.RecognizeFormat(fFileName)));

            targetFile = storePath + storeFile;

            // set paths and links
            var refPath = GKData.GKStoreTypes[(int)MediaStoreType.mstArchive].Sign + targetFile;

            return FileHelper.NormalizeFilename(refPath);
        }

        private void ExtractToFile(string archiveFileName, string targetFileName)
        {
            using (var zip = ZipFile.Open(fArcFileName, ZipArchiveMode.Read, GetZipEncoding())) {
                var entry = zip.GetEntry(targetFileName);
                entry?.ExtractToFile(archiveFileName, true);
            }
        }

        private void ArcFileLoad(string targetFn, Stream toStream)
        {
            targetFn = FileHelper.NormalizeFilename(targetFn);
            using (var zip = ZipFile.Open(fArcFileName, ZipArchiveMode.Read, GetZipEncoding())) {
                var entry = zip.GetEntry(targetFn);
                if (entry != null) {
                    using (var stream = entry.Open()) {
                        stream.CopyTo(toStream);
                    }
                }
            }
        }

        private void ArcFileSave(string fileName, string sfn)
        {
            using (var file = File.Open(fArcFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var z = new ZipArchive(file, ZipArchiveMode.Update, false, GetZipEncoding())) {
                z.CreateEntryFromFile(fileName, sfn, CompressionLevel.Optimal);
            }
        }

        private static Encoding GetZipEncoding()
        {
            int treeVer = 0;
            return (treeVer == 0) ? Encoding.GetEncoding("CP866") : Encoding.UTF8;
        }

        private void ArcFileDelete(string targetFn)
        {
            targetFn = FileHelper.NormalizeFilename(targetFn);

            using (var zip = ZipFile.Open(fArcFileName, ZipArchiveMode.Update, GetZipEncoding())) {
                var entry = zip.GetEntry(targetFn);
                entry?.Delete();
            }
        }

        private bool ArcFileExists(string targetFn)
        {
            targetFn = FileHelper.NormalizeFilename(targetFn);
            using (var zip = ZipFile.Open(fArcFileName, ZipArchiveMode.Read, GetZipEncoding())) {
                return zip.GetEntry(targetFn) != null;
            }
        }

        protected override string NormalizeFileName(BaseContext baseContext)
        {
            throw new NotImplementedException();
        }

    }
}
