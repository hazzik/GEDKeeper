using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GKCore.Types
{
    public sealed class URLMediaStore : IMediaStore
    {
        private readonly string fUrl;

        public URLMediaStore(string fileName)
        {
            fUrl = fileName;
        }

        public Stream MediaLoad(bool throwException)
        {
            using (var webClient = CreateWebClient()) {
                var dataBytes = webClient.DownloadData(fUrl);
                return new MemoryStream(dataBytes);
            }
        }

        public string MediaLoad()
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

        public Task<bool> MediaDelete()
        {
            return Task.FromResult(true);
        }

        public MediaStoreStatus VerifyMediaFile(out string fileName)
        {
            fileName = fUrl;
            return MediaStoreStatus.mssBadData;
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

        public bool MediaSave(BaseContext baseContext, out string refPath)
        {
            // set paths and links
            refPath = fUrl;

            // verify existence
            var alreadyExists = baseContext.MediaExists(refPath);
            if (alreadyExists) {
                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileWithSameNameAlreadyExists));
                return false;
            }

            return true;
        }
    }
}
