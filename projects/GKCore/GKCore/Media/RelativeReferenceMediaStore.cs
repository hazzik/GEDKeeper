using System;
using System.IO;
using BSLib;

namespace GKCore.Types
{
    public sealed class RelativeReferenceMediaStore : FileSystemMediaStore
    {
        public RelativeReferenceMediaStore(BaseContext baseContext, string fileName, bool allowDelete)
            : base(BaseContext.GetTreePath(baseContext.FileName), fileName, allowDelete)
        {
        }

        protected override string NormalizeFileName(BaseContext baseContext)
        {
            var targetFile = baseContext.GetTreeRelativePath(FileName);
            var refPath = GKData.GKStoreTypes[(int)MediaStoreType.mstRelativeReference].Sign + targetFile;

            return FileHelper.NormalizeFilename(refPath);
        }
    }
}
