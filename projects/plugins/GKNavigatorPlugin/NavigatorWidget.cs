﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2023 by Sergey V. Zhdanovskih.
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
using System.Windows.Forms;
using GDModel;
using GKCore;
using GKCore.Interfaces;

namespace GKNavigatorPlugin
{
    public partial class NavigatorWidget : Form, IWidgetForm
    {
        private readonly Plugin fPlugin;
        private readonly ILangMan fLangMan;

        private IBaseWindow fBase;
        private string fDatabaseName;

        private TreeNode tnRoot;
        private TreeNode tnRecAct;
        private TreeNode tnJumpHist;
        private TreeNode tnProblems;
        private TreeNode tnFilters;
        private TreeNode tnBookmarks;
        private TreeNode tnRecords;
        private TreeNode tnRecsIndividual;
        private TreeNode tnRecsFamily;
        private TreeNode tnRecsNote;
        private TreeNode tnRecsMultimedia;
        private TreeNode tnRecsSource;
        private TreeNode tnRecsRepository;
        private TreeNode tnRecsGroup;
        private TreeNode tnRecsResearch;
        private TreeNode tnRecsTask;
        private TreeNode tnRecsCommunication;
        private TreeNode tnRecsLocation;
        private TreeNode tnLanguages;
        private TreeNode tnAssociations;

        public NavigatorWidget(Plugin plugin)
        {
            InitializeComponent();

            fPlugin = plugin;
            fLangMan = fPlugin.LangMan;

            InitControls();
            SetLocale();
        }

        private void InitControls()
        {
            treeView1.Nodes.Clear();

            tnRoot = treeView1.Nodes.Add("root");

            tnRecAct = CreateNode(tnRoot, "Recent Activity", DataCategory.RecentActivity);
            tnJumpHist = CreateNode(tnRecAct, "Jump history", DataCategory.JumpHistory);
            tnProblems = CreateNode(tnRecAct, "Potencial problems", DataCategory.PotencialProblems);
            tnFilters = CreateNode(tnRecAct, "Filters", DataCategory.Filters);

            tnBookmarks = CreateNode(tnRoot, "Bookmarks", DataCategory.Bookmarks);
            tnLanguages = CreateNode(tnRoot, "Languages", DataCategory.Languages);
            tnAssociations = CreateNode(tnRoot, "Associations", DataCategory.Associations);

            tnRecords = tnRoot.Nodes.Add("Records");
            tnRecsIndividual = CreateNode(tnRecords, "Individuals", GDMRecordType.rtIndividual);
            tnRecsFamily = CreateNode(tnRecords, "Families", GDMRecordType.rtFamily);
            tnRecsNote = CreateNode(tnRecords, "Notes", GDMRecordType.rtNote);
            tnRecsMultimedia = CreateNode(tnRecords, "Multimedia", GDMRecordType.rtMultimedia);
            tnRecsSource = CreateNode(tnRecords, "Sources", GDMRecordType.rtSource);
            tnRecsRepository = CreateNode(tnRecords, "Repositories", GDMRecordType.rtRepository);
            tnRecsGroup = CreateNode(tnRecords, "Groups", GDMRecordType.rtGroup);
            tnRecsResearch = CreateNode(tnRecords, "Researches", GDMRecordType.rtResearch);
            tnRecsTask = CreateNode(tnRecords, "Tasks", GDMRecordType.rtTask);
            tnRecsCommunication = CreateNode(tnRecords, "Communications", GDMRecordType.rtCommunication);
            tnRecsLocation = CreateNode(tnRecords, "Locations", GDMRecordType.rtLocation);
        }

        public void SetLocale()
        {
            Text = fLangMan.LS(PLS.LSID_Navigator);
            tnRecAct.Text = fLangMan.LS(PLS.LSID_RecentActivity);
            tnJumpHist.Text = fLangMan.LS(PLS.LSID_JumpHistory);
            tnProblems.Text = fLangMan.LS(PLS.LSID_PotencialProblems);
            tnFilters.Text = fLangMan.LS(PLS.LSID_Filters);
            tnBookmarks.Text = fLangMan.LS(PLS.LSID_Bookmarks);
            tnLanguages.Text = fLangMan.LS(PLS.LSID_Languages);
            tnAssociations.Text = fLangMan.LS(PLS.LSID_Associations);
            tnRecords.Text = fLangMan.LS(PLS.LSID_Records);
        }

        private void Form_Load(object sender, EventArgs e)
        {
            AppHost.Instance.WidgetLocate(this, WidgetLocation.HRight | WidgetLocation.VBottom);
            fPlugin.Host.WidgetShow(fPlugin);
            BaseChanged(fPlugin.Host.GetCurrentFile());
        }

        private void Form_Closed(object sender, EventArgs e)
        {
            fPlugin.Data.SelectLanguage(fBase, GDMLanguageID.Unknown);
            BaseChanged(null);
            fPlugin.Host.WidgetClose(fPlugin);
            fPlugin.CloseForm();
        }

        public void BaseChanged(IBaseWindow baseWin)
        {
            if (fBase != baseWin && fBase != null) {
            }

            fBase = baseWin;

            fDatabaseName = (fBase == null) ? string.Empty : Path.GetFileName(fBase.Context.FileName);

            UpdateControls();
        }

        public void BaseClosed(IBaseWindow baseWin)
        {
            fPlugin.Data.CloseBase(baseWin.Context.FileName);
            fDatabaseName = "";
            fBase = null;
            UpdateControls(true);
        }

        private static string FmtTitle(string title, int count)
        {
            return string.Format("{0} ({1})", title, count);
        }

        private void UpdateControls(bool afterClose = false)
        {
            try {
                string dbName;
                int[] stats;

                if (fBase == null) {
                    dbName = "";
                    stats = new int[((int)GDMRecordType.rtLast)];
                } else {
                    dbName = fDatabaseName;
                    stats = fBase.Context.Tree.GetRecordStats();
                }

                try {
                    treeView1.BeginUpdate();

                    tnRoot.Text = dbName;
                    tnRecsIndividual.Text = FmtTitle(fLangMan.LS(PLS.LSID_Individuals), stats[(int)GDMRecordType.rtIndividual]);
                    tnRecsFamily.Text = FmtTitle(fLangMan.LS(PLS.LSID_Families), stats[(int)GDMRecordType.rtFamily]);
                    tnRecsNote.Text = FmtTitle(fLangMan.LS(PLS.LSID_Notes), stats[(int)GDMRecordType.rtNote]);
                    tnRecsMultimedia.Text = FmtTitle(fLangMan.LS(PLS.LSID_Multimedia), stats[(int)GDMRecordType.rtMultimedia]);
                    tnRecsSource.Text = FmtTitle(fLangMan.LS(PLS.LSID_Sources), stats[(int)GDMRecordType.rtSource]);
                    tnRecsRepository.Text = FmtTitle(fLangMan.LS(PLS.LSID_Repositories), stats[(int)GDMRecordType.rtRepository]);
                    tnRecsGroup.Text = FmtTitle(fLangMan.LS(PLS.LSID_Groups), stats[(int)GDMRecordType.rtGroup]);
                    tnRecsResearch.Text = FmtTitle(fLangMan.LS(PLS.LSID_Researches), stats[(int)GDMRecordType.rtResearch]);
                    tnRecsTask.Text = FmtTitle(fLangMan.LS(PLS.LSID_Tasks), stats[(int)GDMRecordType.rtTask]);
                    tnRecsCommunication.Text = FmtTitle(fLangMan.LS(PLS.LSID_Communications), stats[(int)GDMRecordType.rtCommunication]);
                    tnRecsLocation.Text = FmtTitle(fLangMan.LS(PLS.LSID_Locations), stats[(int)GDMRecordType.rtLocation]);

                    treeView1.ExpandAll();
                } finally {
                    treeView1.EndUpdate();
                }
            } catch (Exception ex) {
                Logger.WriteError("GKNavigatorPlugin.UpdateControls()", ex);
            }
        }

        private TreeNode CreateNode(TreeNode parent, string title, object tag)
        {
            TreeNode result = parent.Nodes.Add(title);
            result.Tag = tag;
            return result;
        }

        private void TreeView1AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;
            object tag = e.Node.Tag;
            if (tag == null) return;
            fPlugin.Data.ShowItem(fBase, tag, lvData);
        }

        private void lvData_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            object tag = treeView1.SelectedNode.Tag;
            var itemData = lvData.GetSelectedData();
            fPlugin.Data.SelectItem(fBase, tag, itemData);
        }
    }
}
