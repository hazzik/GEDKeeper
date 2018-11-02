/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2018 by Sergey V. Zhdanovskih.
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
using BSLib;
using GKCommon.GEDCOM;
using GKCore.Interfaces;
using GKCore.MVP;
using GKCore.MVP.Views;

namespace GKCore.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class PortraitSelectDlgController : EditorController<GEDCOMMultimediaLink, IPortraitSelectDlg>
    {
        public PortraitSelectDlgController(IPortraitSelectDlg view) : base(view)
        {
        }

        public override bool Accept()
        {
            try {
                ExtRect selectRegion = fView.ImageCtl.SelectionRegion;

                if (!selectRegion.IsEmpty()) {
                    fModel.IsPrimaryCutout = true;
                    fModel.CutoutPosition.Value = selectRegion;
                } else {
                    fModel.IsPrimaryCutout = false;
                    fModel.CutoutPosition.Value = ExtRect.CreateEmpty();
                }

                PortraitsCache.Instance.RemoveObsolete(fModel);

                return true;
            } catch (Exception ex) {
                Logger.LogWrite("PortraitSelectDlgController.Accept(): " + ex.Message);
                return false;
            }
        }

        public override void UpdateView()
        {
            if (fModel == null || fModel.Value == null) return;

            GEDCOMMultimediaRecord mmRec = (GEDCOMMultimediaRecord)fModel.Value;

            IImage img = fBase.Context.LoadMediaImage(mmRec.FileReferences[0], false);
            if (img == null) return;

            fView.ImageCtl.OpenImage(img);

            if (fModel.IsPrimaryCutout) {
                fView.ImageCtl.SelectionRegion = fModel.CutoutPosition.Value;
            }
        }
    }
}
