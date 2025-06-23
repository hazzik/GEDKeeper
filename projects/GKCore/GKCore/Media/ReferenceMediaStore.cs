using BSLib;
using GKCore.Interfaces;

namespace GKCore.Types
{
    public sealed class ReferenceMediaStore : FileSystemMediaStore
    {
        public ReferenceMediaStore(string fileName, bool allowDelete)
            : base(string.Empty, fileName, allowDelete)
        {
        }

        protected override string NormalizeFileName(IBaseContext baseContext)
        {
            return FileHelper.NormalizeFilename(FileName);
        }
    }
}
