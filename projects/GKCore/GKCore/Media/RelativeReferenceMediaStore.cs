using BSLib;
using GKCore.Interfaces;

namespace GKCore.Types
{
    public sealed class RelativeReferenceMediaStore : FileSystemMediaStore
    {
        public RelativeReferenceMediaStore(IBaseContext baseContext, string fileName, bool allowDelete)
            : base(baseContext.GetTreePath(), fileName, allowDelete)
        {
        }

        protected override string NormalizeFileName(IBaseContext baseContext)
        {
            var targetFile = baseContext.GetTreeRelativePath(FileName);
            return FileHelper.NormalizeFilename(targetFile);
        }

        protected override string CreateRefPath(string targetFile)
        {
            return GKData.GKStoreTypes[(int)MediaStoreType.mstRelativeReference].Sign + targetFile;
        }
    }
}
