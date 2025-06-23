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

        protected override string LoadFileCore(string fileName)
        {
            var tempFile = GKUtils.GetTempDir() + Path.GetFileName(fileName);

            using (var zip = ZipFile.Open(fArcFileName, ZipArchiveMode.Read, GetZipEncoding())) {
                var entry = zip.GetEntry(fileName);
                entry?.ExtractToFile(tempFile, true);
            }

            return tempFile;
        }

        protected override bool DeleteCore(string fileName)
        {
            ArcFileDelete(fileName);
            return true;
        }

        public override MediaStoreStatus VerifyMediaFile(out string fileName)
        {
            var result = MediaStoreStatus.mssBadData;
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
            string storePath = GKUtils.GetStoreFolder(GKUtils.GetMultimediaKind(GDMFileReference.RecognizeFormat(fFileName)));

            targetFile = FileHelper.NormalizeFilename(storePath + storeFile);

            // set paths and links
            return GKData.GKStoreTypes[(int)MediaStoreType.mstArchive].Sign + targetFile;
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
