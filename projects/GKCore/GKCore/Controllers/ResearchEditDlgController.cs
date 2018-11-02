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
using GKCommon.GEDCOM;
using GKCore.MVP;
using GKCore.MVP.Views;
using GKCore.Types;

namespace GKCore.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ResearchEditDlgController : EditorController<GEDCOMResearchRecord, IResearchEditDlg>
    {
        public ResearchEditDlgController(IResearchEditDlg view) : base(view)
        {
            for (GKResearchPriority rp = GKResearchPriority.rpNone; rp <= GKResearchPriority.rpTop; rp++) {
                fView.Priority.Add(LangMan.LS(GKData.PriorityNames[(int)rp]));
            }

            for (GKResearchStatus rs = GKResearchStatus.rsDefined; rs <= GKResearchStatus.rsWithdrawn; rs++) {
                fView.Status.Add(LangMan.LS(GKData.StatusNames[(int)rs]));
            }
        }

        public override bool Accept()
        {
            try {
                fModel.ResearchName = fView.Name.Text;
                fModel.Priority = (GKResearchPriority)fView.Priority.SelectedIndex;
                fModel.Status = (GKResearchStatus)fView.Status.SelectedIndex;
                fModel.StartDate.Assign(GEDCOMDate.CreateByFormattedStr(fView.StartDate.Text, true));
                fModel.StopDate.Assign(GEDCOMDate.CreateByFormattedStr(fView.StopDate.Text, true));
                fModel.Percent = int.Parse(fView.Percent.Text);

                fLocalUndoman.Commit();

                fBase.NotifyRecord(fModel, RecordAction.raEdit);

                return true;
            } catch (Exception ex) {
                Logger.LogWrite("ResearchEditDlgController.Accept(): " + ex.Message);
                return false;
            }
        }

        public override void UpdateView()
        {
            if (fModel == null) {
                fView.Name.Text = "";
                fView.Priority.SelectedIndex = -1;
                fView.Status.SelectedIndex = -1;
                fView.StartDate.Text = "";
                fView.StopDate.Text = "";
                fView.Percent.Value = 0;
            } else {
                fView.Name.Text = fModel.ResearchName;
                fView.Priority.SelectedIndex = (int)fModel.Priority;
                fView.Status.SelectedIndex = (int)fModel.Status;
                fView.StartDate.Text = fModel.StartDate.GetDisplayString(DateFormat.dfDD_MM_YYYY);
                fView.StopDate.Text = fModel.StopDate.GetDisplayString(DateFormat.dfDD_MM_YYYY);
                fView.Percent.Value = fModel.Percent;
            }

            fView.NotesList.ListModel.DataOwner = fModel;
            fView.TasksList.ListModel.DataOwner = fModel;
            fView.CommunicationsList.ListModel.DataOwner = fModel;
            fView.GroupsList.ListModel.DataOwner = fModel;
        }

        public void JumpToRecord(GEDCOMRecord record)
        {
            if (record != null && Accept()) {
                fBase.SelectRecordByXRef(record.XRef);
                fView.Close();
            }
        }
    }
}
