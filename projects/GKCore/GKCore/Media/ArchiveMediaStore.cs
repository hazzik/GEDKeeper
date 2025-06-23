using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using BSLib;
using GDModel;
using GKCore.Interfaces;

namespace GKCore.Types
{
    public sealed class ArchiveMediaStore : MediaStore
    {
        private readonly string fArcFileName;
        private readonly string fFileName;

        public ArchiveMediaStore(IBaseContext baseContext, string fileName, bool allowDelete) : base(allowDelete)
        {
            fFileName = fileName;
            fArcFileName = baseContext.GetArcFileName();
        }

        protected override Stream LoadStreamCore(string fileName)
        {
            var targetFn = FileHelper.NormalizeFilename(fileName);
            using (var zip = ZipFile.Open(fArcFileName, ZipArchiveMode.Read, GetZipEncoding())) {
                var entry = zip.GetEntry(targetFn);
                if (entry != null) {
                    using (var archStream = entry.Open()) {
                        var memoryStream = new MemoryStream();
                        archStream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        return memoryStream;
                    }
                }
            }

            return null;
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
            var targetFn = FileHelper.NormalizeFilename(fileName);
            using (var zip = ZipFile.Open(fArcFileName, ZipArchiveMode.Update, GetZipEncoding())) {
                var entry = zip.GetEntry(targetFn);
                entry?.Delete();
            }

            return true;
        }

        public override MediaStoreStatus VerifyMediaFile(out string fileName)
        {
            var result = MediaStoreStatus.mssBadData;
            try {
                fileName = FileHelper.NormalizeFilename(fFileName);
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

        protected override bool SaveCopy(IBaseContext baseContext, string targetFile)
        {
            // save a copy to archive
            using (var file = File.Open(fArcFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var z = new ZipArchive(file, ZipArchiveMode.Update, false, GetZipEncoding())) {
                z.CreateEntryFromFile(fFileName, targetFile, CompressionLevel.Optimal);
            }

            return true;
        }

        protected override string CreateRefPath(string fileName)
        {
            return GKData.GKStoreTypes[(int)MediaStoreType.mstArchive].Sign + fileName;
        }

        protected override string NormalizeFileName(IBaseContext baseContext)
        {
            var storeFile = Path.GetFileName(fFileName);
            var storePath = GKUtils.GetStoreFolder(GKUtils.GetMultimediaKind(GDMFileReference.RecognizeFormat(fFileName)));

            return FileHelper.NormalizeFilename(storePath + storeFile);
        }

        private static Encoding GetZipEncoding()
        {
            int treeVer = 0;
            return (treeVer == 0) ? Encoding.GetEncoding("CP866") : Encoding.UTF8;
        }

        private bool ArcFileExists(string targetFn)
        {
            targetFn = FileHelper.NormalizeFilename(targetFn);
            using (var zip = ZipFile.Open(fArcFileName, ZipArchiveMode.Read, GetZipEncoding())) {
                return zip.GetEntry(targetFn) != null;
            }
        }
    }
}
