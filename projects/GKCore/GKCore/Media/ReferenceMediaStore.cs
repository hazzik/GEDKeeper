using BSLib;

namespace GKCore.Types
{
    public sealed class ReferenceMediaStore : FileSystemMediaStore
    {
        public ReferenceMediaStore(string fileName, bool allowDelete)
            : base(string.Empty, fileName, allowDelete)
        {
        }

        protected override string NormalizeFileName(BaseContext baseContext)
        {
            return FileHelper.NormalizeFilename(FileName);
        }
    }
}
