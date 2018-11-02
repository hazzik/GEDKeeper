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

#define SEX_SYMBOLS

using System;
using GKCommon.GEDCOM;
using GKCore.Interfaces;
using GKCore.MVP;
using GKCore.MVP.Views;
using GKCore.Operations;
using GKCore.Options;
using GKCore.Types;

namespace GKCore.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PersonEditDlgController : EditorController<GEDCOMIndividualRecord, IPersonEditDlg>
    {
        private IImage fPortraitImg;
        private GEDCOMIndividualRecord fTarget;
        private TargetMode fTargetMode;

        public GEDCOMIndividualRecord Target
        {
            get { return fTarget; }
            set { SetTarget(value); }
        }

        public TargetMode TargetMode
        {
            get { return fTargetMode; }
            set { fTargetMode = value; }
        }


        public PersonEditDlgController(IPersonEditDlg view) : base(view)
        {
            for (GEDCOMRestriction res = GEDCOMRestriction.rnNone; res <= GEDCOMRestriction.rnPrivacy; res++) {
                fView.RestrictionCombo.Add(LangMan.LS(GKData.Restrictions[(int)res]));
            }

            for (GEDCOMSex sx = GEDCOMSex.svNone; sx <= GEDCOMSex.svUndetermined; sx++) {
                string name = GKUtils.SexStr(sx);
                IImage image = null;
                #if SEX_SYMBOLS
                switch (sx) {
                    case GEDCOMSex.svMale:
                        image = AppHost.GfxProvider.LoadResourceImage("sym_male.png", true);
                        break;
                    case GEDCOMSex.svFemale:
                        image = AppHost.GfxProvider.LoadResourceImage("sym_female.png", true);
                        break;
                }
                #endif
                fView.SexCombo.AddItem(name, null, image);
            }
        }

        private bool IsExtendedWomanSurname()
        {
            bool result = (GlobalOptions.Instance.WomanSurnameFormat != WomanSurnameFormat.wsfNotExtend) &&
                (fView.SexCombo.SelectedIndex == (sbyte)GEDCOMSex.svFemale);
            return result;
        }

        public void ChangeSex()
        {
            if (!IsExtendedWomanSurname()) {
                fView.SurnameLabel.Text = LangMan.LS(LSID.LSID_Surname);
                fView.MarriedSurname.Enabled = false;
            } else {
                fView.SurnameLabel.Text = LangMan.LS(LSID.LSID_MaidenSurname);
                fView.MarriedSurname.Enabled = true;
            }

            UpdatePortrait(true);
        }

        public override bool Accept()
        {
            try {
                GEDCOMPersonalName np = fModel.PersonalNames[0];
                GKUtils.SetNameParts(np, fView.Surname.Text, fView.Name.Text, fView.Patronymic.Text);

                GEDCOMPersonalNamePieces pieces = np.Pieces;
                pieces.Nickname = fView.Nickname.Text;
                pieces.Prefix = fView.NamePrefix.Text;
                pieces.SurnamePrefix = fView.SurnamePrefix.Text;
                pieces.Suffix = fView.NameSuffix.Text;
                if (IsExtendedWomanSurname()) {
                    pieces.MarriedName = fView.MarriedSurname.Text;
                }

                fModel.Sex = (GEDCOMSex)fView.SexCombo.SelectedIndex;
                fModel.Patriarch = fView.Patriarch.Checked;
                fModel.Bookmark = fView.Bookmark.Checked;
                fModel.Restriction = (GEDCOMRestriction)fView.RestrictionCombo.SelectedIndex;

                if (fModel.ChildToFamilyLinks.Count > 0) {
                    fModel.ChildToFamilyLinks[0].Family.SortChilds();
                }

                fLocalUndoman.Commit();

                fBase.NotifyRecord(fModel, RecordAction.raEdit);

                return true;
            } catch (Exception ex) {
                Logger.LogWrite("PersonEditDlgController.Accept(): " + ex.Message);
                return false;
            }
        }

        public override void UpdateView()
        {
            try {
                GEDCOMPersonalName np = (fModel.PersonalNames.Count > 0) ? fModel.PersonalNames[0] : null;
                UpdateNameControls(np);

                fView.SexCombo.SelectedIndex = (sbyte)fModel.Sex;
                fView.Patriarch.Checked = fModel.Patriarch;
                fView.Bookmark.Checked = fModel.Bookmark;

                //cmbRestriction.SelectedIndexChanged -= cbRestriction_SelectedIndexChanged;
                fView.RestrictionCombo.SelectedIndex = (sbyte)fModel.Restriction;
                //cmbRestriction.SelectedIndexChanged += cbRestriction_SelectedIndexChanged;

                fView.EventsList.ListModel.DataOwner = fModel;
                fView.NotesList.ListModel.DataOwner = fModel;
                fView.MediaList.ListModel.DataOwner = fModel;
                fView.SourcesList.ListModel.DataOwner = fModel;
                fView.AssociationsList.ListModel.DataOwner = fModel;

                fView.GroupsList.ListModel.DataOwner = fModel;
                fView.NamesList.ListModel.DataOwner = fModel;
                fView.SpousesList.ListModel.DataOwner = fModel;
                fView.UserRefList.ListModel.DataOwner = fModel;

                UpdateControls(true);
            } catch (Exception ex) {
                Logger.LogWrite("PersonEditDlgController.UpdateView(): " + ex.Message);
            }
        }

        public void UpdateControls(bool totalUpdate = false)
        {
            bool locked = (fView.RestrictionCombo.SelectedIndex == (int)GEDCOMRestriction.rnLocked);

            if (fModel.ChildToFamilyLinks.Count != 0) {
                GEDCOMFamilyRecord family = fModel.ChildToFamilyLinks[0].Family;
                fView.SetParentsAvl(true, locked);

                GEDCOMIndividualRecord relPerson = family.GetHusband();
                if (relPerson != null) {
                    fView.SetFatherAvl(true, locked);
                    fView.Father.Text = GKUtils.GetNameString(relPerson, true, false);
                } else {
                    fView.SetFatherAvl(false, locked);
                    fView.Father.Text = "";
                }

                relPerson = family.GetWife();
                if (relPerson != null) {
                    fView.SetMotherAvl(true, locked);
                    fView.Mother.Text = GKUtils.GetNameString(relPerson, true, false);
                } else {
                    fView.SetMotherAvl(false, locked);
                    fView.Mother.Text = "";
                }
            } else {
                fView.SetParentsAvl(false, locked);
                fView.SetFatherAvl(false, locked);
                fView.SetMotherAvl(false, locked);

                fView.Father.Text = "";
                fView.Mother.Text = "";
            }

            if (totalUpdate) {
                fView.EventsList.UpdateSheet();
                fView.NotesList.UpdateSheet();
                fView.MediaList.UpdateSheet();
                fView.SourcesList.UpdateSheet();
                fView.AssociationsList.UpdateSheet();

                fView.GroupsList.UpdateSheet();
                fView.NamesList.UpdateSheet();
                fView.SpousesList.UpdateSheet();
                fView.UserRefList.UpdateSheet();
            }

            UpdatePortrait(totalUpdate);

            // controls lock
            fView.Name.Enabled = !locked;
            fView.Patronymic.Enabled = !locked;
            fView.Surname.Enabled = !locked;

            fView.SexCombo.Enabled = !locked;
            fView.Patriarch.Enabled = !locked;
            fView.Bookmark.Enabled = !locked;

            fView.NamePrefix.Enabled = !locked;
            fView.Nickname.Enabled = !locked;
            fView.SurnamePrefix.Enabled = !locked;
            fView.NameSuffix.Enabled = !locked;

            fView.EventsList.ReadOnly = locked;
            fView.NotesList.ReadOnly = locked;
            fView.MediaList.ReadOnly = locked;
            fView.SourcesList.ReadOnly = locked;
            fView.SpousesList.ReadOnly = locked;
            fView.AssociationsList.ReadOnly = locked;
            fView.GroupsList.ReadOnly = locked;
            fView.UserRefList.ReadOnly = locked;

            ICulture culture = fBase.Context.Culture;
            fView.Surname.Enabled = fView.Surname.Enabled && culture.HasSurname();
            fView.Patronymic.Enabled = fView.Patronymic.Enabled && culture.HasPatronymic();
        }

        public void UpdateNameControls(GEDCOMPersonalName np)
        {
            if (np != null) {
                var parts = GKUtils.GetNameParts(fModel, np, false);

                fView.Surname.Text = parts.Surname;
                fView.Name.Text = parts.Name;
                fView.Patronymic.Text = parts.Patronymic;

                fView.NamePrefix.Text = np.Pieces.Prefix;
                fView.Nickname.Text = np.Pieces.Nickname;
                fView.SurnamePrefix.Text = np.Pieces.SurnamePrefix;
                fView.NameSuffix.Text = np.Pieces.Suffix;

                fView.MarriedSurname.Text = np.Pieces.MarriedName;
            } else {
                fView.Surname.Text = "";
                fView.Name.Text = "";
                fView.Patronymic.Text = "";

                fView.NamePrefix.Text = "";
                fView.Nickname.Text = "";
                fView.SurnamePrefix.Text = "";
                fView.NameSuffix.Text = "";

                fView.MarriedSurname.Text = "";
            }
        }

        public void UpdatePortrait(bool totalUpdate)
        {
            if (fPortraitImg == null || totalUpdate) {
                fPortraitImg = fBase.Context.GetPrimaryBitmap(fModel, fView.Portrait.Width, fView.Portrait.Height, false);
            }

            IImage img = fPortraitImg;
            if (img == null) {
                // using avatar's image
                GEDCOMSex curSex = (GEDCOMSex)fView.SexCombo.SelectedIndex;

                switch (curSex) {
                    case GEDCOMSex.svMale:
                        img = AppHost.GfxProvider.LoadResourceImage("pi_male_140.png", false);
                        break;

                    case GEDCOMSex.svFemale:
                        img = AppHost.GfxProvider.LoadResourceImage("pi_female_140.png", false);
                        break;

                    default:
                        break;
                }
            }
            fView.SetPortrait(img);

            bool locked = (fView.RestrictionCombo.SelectedIndex == (int)GEDCOMRestriction.rnLocked);
            fView.SetPortraitAvl((fPortraitImg != null), locked);
        }

        private void SetTarget(GEDCOMIndividualRecord value)
        {
            try {
                fTarget = value;

                if (fTarget != null) {
                    ICulture culture = fBase.Context.Culture;
                    INamesTable namesTable = AppHost.NamesTable;

                    var parts = GKUtils.GetNameParts(fTarget);
                    fView.Surname.Text = parts.Surname;
                    GEDCOMSex sx = (GEDCOMSex)fView.SexCombo.SelectedIndex;

                    switch (fTargetMode) {
                        case TargetMode.tmParent:
                            if (sx == GEDCOMSex.svFemale) {
                                SetMarriedSurname(parts.Surname);
                            }
                            if (culture.HasPatronymic()) {
                                fView.Patronymic.Add(namesTable.GetPatronymicByName(parts.Name, GEDCOMSex.svMale));
                                fView.Patronymic.Add(namesTable.GetPatronymicByName(parts.Name, GEDCOMSex.svFemale));
                                fView.Patronymic.Text = namesTable.GetPatronymicByName(parts.Name, sx);
                            }
                            break;

                        case TargetMode.tmChild:
                            switch (sx) {
                                case GEDCOMSex.svMale:
                                    if (culture.HasPatronymic()) {
                                        fView.Name.Text = namesTable.GetNameByPatronymic(parts.Patronymic);
                                    }
                                    break;

                                case GEDCOMSex.svFemale:
                                    SetMarriedSurname(parts.Surname);
                                    break;
                            }
                            break;

                        case TargetMode.tmWife:
                            SetMarriedSurname(parts.Surname);
                            break;
                    }
                }
            } catch (Exception ex) {
                Logger.LogWrite("PersonEditDlg.SetTarget(" + fTargetMode.ToString() + "): " + ex.Message);
            }
        }

        private void SetMarriedSurname(string husbSurname)
        {
            string surname = fBase.Context.Culture.GetMarriedSurname(husbSurname);
            if (IsExtendedWomanSurname()) {
                fView.MarriedSurname.Text = surname;
            } else {
                fView.Surname.Text = "(" + surname + ")";
            }
        }

        public void AcceptTempData()
        {
            // It is very important for some methods
            // For the sample: we need to have gender's value on time of call AddSpouse (for define husband/wife)
            // And we need to have actual name's value for visible it in FamilyEditDlg

            fLocalUndoman.DoOrdinaryOperation(OperationType.otIndividualSexChange, fModel, (GEDCOMSex)fView.SexCombo.SelectedIndex);
            fLocalUndoman.DoIndividualNameChange(fModel, fView.Surname.Text, fView.Name.Text, fView.Patronymic.Text);
        }

        public void AddPortrait()
        {
            if (BaseController.AddIndividualPortrait(fBase, fLocalUndoman, fModel)) {
                fView.MediaList.UpdateSheet();
                UpdatePortrait(true);
            }
        }

        public void DeletePortrait()
        {
            if (BaseController.DeleteIndividualPortrait(fBase, fLocalUndoman, fModel)) {
                UpdatePortrait(true);
            }
        }

        public void AddParents()
        {
            AcceptTempData();

            GEDCOMFamilyRecord family = fBase.Context.SelectFamily(fModel);
            if (family == null) return;

            if (family.IndexOfChild(fModel) < 0) {
                fLocalUndoman.DoOrdinaryOperation(OperationType.otIndividualParentsAttach, fModel, family);
            }
            UpdateControls();
        }

        public void EditParents()
        {
            AcceptTempData();

            GEDCOMFamilyRecord family = fBase.Context.GetChildFamily(fModel, false, null);
            if (family != null && BaseController.ModifyFamily(fBase, ref family, TargetMode.tmNone, null)) {
                UpdateControls();
            }
        }

        public void DeleteParents()
        {
            if (!AppHost.StdDialogs.ShowQuestionYN(LangMan.LS(LSID.LSID_DetachParentsQuery))) return;

            GEDCOMFamilyRecord family = fBase.Context.GetChildFamily(fModel, false, null);
            if (family == null) return;

            fLocalUndoman.DoOrdinaryOperation(OperationType.otIndividualParentsDetach, fModel, family);
            UpdateControls();
        }

        public void AddFather()
        {
            if (BaseController.AddIndividualFather(fBase, fLocalUndoman, fModel)) {
                UpdateControls();
            }
        }

        public void DeleteFather()
        {
            if (BaseController.DeleteIndividualFather(fBase, fLocalUndoman, fModel)) {
                UpdateControls();
            }
        }

        public void AddMother()
        {
            if (BaseController.AddIndividualMother(fBase, fLocalUndoman, fModel)) {
                UpdateControls();
            }
        }

        public void DeleteMother()
        {
            if (BaseController.DeleteIndividualMother(fBase, fLocalUndoman, fModel)) {
                UpdateControls();
            }
        }

        public void JumpToRecord(GEDCOMRecord record)
        {
            if (record != null && Accept()) {
                fBase.SelectRecordByXRef(record.XRef);
                fView.Close();
            }
        }

        public void JumpToFather()
        {
            GEDCOMFamilyRecord family = fBase.Context.GetChildFamily(fModel, false, null);
            if (family == null) return;

            JumpToRecord(family.GetHusband());
        }

        public void JumpToMother()
        {
            GEDCOMFamilyRecord family = fBase.Context.GetChildFamily(fModel, false, null);
            if (family == null) return;

            JumpToRecord(family.GetWife());
        }
    }
}
