using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GKCore.Types
{
    public sealed class URLMediaStore : MediaStore
    {
        private readonly string fUrl;

        public URLMediaStore(string fileName) : base(false)
        {
            fUrl = fileName;
        }

        public override Stream MediaLoad(bool throwException)
        {
            using (var webClient = CreateWebClient()) {
                var dataBytes = webClient.DownloadData(fUrl);
                return new MemoryStream(dataBytes);
            }
        }

        public override string MediaLoad()
        {
            string fileName;
            try {
                fileName = GKUtils.GetTempDir() + Path.GetFileName(fUrl);
                using (var webClient = CreateWebClient()) {
                    webClient.DownloadFile(fUrl, fileName);
                }
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.MediaLoad_fn()", ex);
                return "";
            }

            return fileName;
        }

        public override MediaStoreStatus VerifyMediaFile(out string fileName)
        {
            fileName = fUrl;
            return MediaStoreStatus.mssBadData;
        }

        protected override bool DeleteCore(string fileName)
        {
            throw new NotSupportedException();
        }

        protected override string NormalizeFileName(BaseContext baseContext)
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
