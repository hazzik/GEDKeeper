using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GKCore.Interfaces;

namespace GKCore.Types
{
    public sealed class URLMediaStore : MediaStore
    {
        private readonly string fUrl;

        public URLMediaStore(string fileName) : base(false)
        {
            fUrl = fileName;
        }

        protected override Stream LoadStreamCore(string fileName)
        {
            using (var webClient = CreateWebClient()) {
                var dataBytes = webClient.DownloadData(fUrl);
                var memoryStream = new MemoryStream(dataBytes);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
        }

        protected override string LoadFileCore(string fileName)
        {
            var tempFile = GKUtils.GetTempDir() + Path.GetFileName(fileName);
            using (var webClient = CreateWebClient()) {
                webClient.DownloadFile(fUrl, tempFile);
            }

            return tempFile;
        }

        public override MediaStoreStatus VerifyMediaFile(out string fileName)
        {
            fileName = fUrl;
            return MediaStoreStatus.mssExists;
        }

        protected override bool DeleteCore(string fileName)
        {
            throw new NotSupportedException();
        }

        protected override string CreateRefPath(string targetFile)
        {
            return targetFile;
        }

        protected override string NormalizeFileName(IBaseContext baseContext)
        {
            return fUrl;
        }

        private static WebClient CreateWebClient()
        {
            WebClient webClient = null;
            try {
                GKUtils.InitSecurityProtocol();
                webClient = new WebClient();
                webClient.Headers["User-Agent"] = string.Format("{0}/{1}", GKData.APP_TITLE, GKData.APP_VERSION);
                return webClient;
            } catch {
                webClient?.Dispose();
                throw;
            }
        }
    }
}
