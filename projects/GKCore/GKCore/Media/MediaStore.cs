/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2017 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Threading.Tasks;
using GKCore.Options;

namespace GKCore.Types
{
    /// <summary />
    public abstract class MediaStore
    {
        private readonly bool fAllowDelete;

        protected MediaStore(bool allowDelete)
        {
            fAllowDelete = allowDelete;
        }

        public string MediaLoad()
        {
            try {
                var storeStatus = VerifyMediaFile(out var fileName);
                if (storeStatus == MediaStoreStatus.mssExists) {
                    return LoadFileCore(fileName);
                }

                var errorMessage = ErrorMessage(storeStatus, fileName);
                if (errorMessage != null) {
                    AppHost.StdDialogs.ShowError(errorMessage);
                }
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.MediaLoad_fn()", ex);
            }

            return string.Empty;
        }

        public Stream MediaLoad(bool throwException)
        {
            var status = VerifyMediaFile(out var fileName);
            if (status == MediaStoreStatus.mssExists) {
                return LoadStreamCore(fileName);
            }

            if (throwException) {
                throw new MediaFileNotFoundException(fileName);
            }

            var errorMessage = ErrorMessage(status, fileName);
            if (errorMessage != null) {
                AppHost.StdDialogs.ShowError(errorMessage);
            }

            return null;
        }

        public abstract MediaStoreStatus VerifyMediaFile(out string fileName);

        public async Task<bool> MediaDelete()
        {
            if (!fAllowDelete) {
                return true;
            }

            try {
                var storeStatus = VerifyMediaFile(out var fileName);

                switch (storeStatus) {
                    case MediaStoreStatus.mssExists:
                        return await ConfirmDelete() && DeleteCore(fileName);
                    case MediaStoreStatus.mssBadData:
                        return true;
                    default:
                        var errorMessage = ErrorMessage(storeStatus, fileName);
                        return errorMessage != null &&
                               await AppHost.StdDialogs.ShowQuestion(LangMan.LS(LSID.ContinueQuestion, errorMessage));
                }
            } catch (Exception ex) {
                Logger.WriteError("BaseContext.MediaDelete()", ex);
                return false;
            }
        }

        private static async Task<bool> ConfirmDelete()
        {
            if (GlobalOptions.Instance.DeleteMediaFileWithoutConfirm) {
                return true;
            }

            // TODO: may be Yes/No/Cancel?
            return await AppHost.StdDialogs.ShowQuestion(LangMan.LS(LSID.MediaFileDeleteQuery));
        }

        private static string ErrorMessage(MediaStoreStatus storeStatus, string fileName)
        {
            switch (storeStatus) {
                case MediaStoreStatus.mssFileNotFound:
                    return LangMan.LS(LSID.FileNotFound, fileName);
                case MediaStoreStatus.mssStgNotFound:
                    return LangMan.LS(LSID.StgNotFound);
                case MediaStoreStatus.mssArcNotFound:
                    return LangMan.LS(LSID.ArcNotFound);
            }

            return null;
        }


        public virtual bool MediaSave(BaseContext baseContext, out string refPath)
        {
            // set paths and links
            refPath = NormalizeFileName(baseContext);

            // verify existence
            bool alreadyExists = baseContext.MediaExists(refPath);
            if (alreadyExists) {
                AppHost.StdDialogs.ShowError(LangMan.LS(LSID.FileWithSameNameAlreadyExists));
                return false;
            }

            return true;
        }

        protected abstract string NormalizeFileName(BaseContext baseContext);
        protected abstract string LoadFileCore(string fileName);
        protected abstract Stream LoadStreamCore(string fileName);
        protected abstract bool DeleteCore(string fileName);
    }
}
