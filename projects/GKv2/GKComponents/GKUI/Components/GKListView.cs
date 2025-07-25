﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2025 by Sergey V. Zhdanovskih.
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using GKCore;
using GKCore.Design;
using GKCore.Design.Controls;
using GKCore.Design.Graphics;
using GKCore.Interfaces;
using GKUI.Platform.Handlers;
using BSDListItem = GKCore.Design.Controls.IListItem;
using BSDSortOrder = GKCore.Design.BSDTypes.SortOrder;

namespace GKUI.Components
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class GKListItem : ListViewItem, BSDListItem
    {
        protected object fValue;

        public GKListItem(object itemValue, object tag)
        {
            fValue = itemValue;
            Text = ToString();
            Tag = tag;
        }

        public override string ToString()
        {
            return (fValue == null) ? string.Empty : fValue.ToString();
        }

        public int CompareTo(object obj)
        {
            GKListItem otherItem = obj as GKListItem;
            if (otherItem == null) {
                return -1;
            }

            IComparable cv1 = fValue as IComparable;
            IComparable cv2 = otherItem.fValue as IComparable;

            int compRes;
            if (cv1 != null && cv2 != null) {
                compRes = cv1.CompareTo(cv2);
            } else if (cv1 != null) {
                compRes = -1;
            } else if (cv2 != null) {
                compRes = 1;
            } else {
                compRes = 0;
            }
            return compRes;
        }

        public void SetSubItem(int index, object value)
        {
            SubItems[index] = new GKListSubItem(value);
        }

        public void SetBackColor(IColor color)
        {
            var colorHandler = color as ColorHandler;
            if (colorHandler != null) {
                BackColor = colorHandler.Handle;
            }
        }

        public void SetForeColor(IColor color)
        {
            var colorHandler = color as ColorHandler;
            if (colorHandler != null) {
                ForeColor = colorHandler.Handle;
            }
        }
    }


    public class GKListSubItem : ListViewItem.ListViewSubItem, IComparable
    {
        protected object fValue;

        public GKListSubItem(object itemValue)
        {
            fValue = itemValue;
            Text = ToString();
        }

        public override string ToString()
        {
            return (fValue == null) ? string.Empty : fValue.ToString();
        }

        public int CompareTo(object obj)
        {
            var otherItem = obj as GKListSubItem;
            if (otherItem == null) {
                return -1;
            }

            if (fValue is string && otherItem.fValue is string) {
                return GKUtils.StrCompareEx((string)fValue, (string)otherItem.fValue);
            }

            IComparable cv1 = fValue as IComparable;
            IComparable cv2 = otherItem.fValue as IComparable;

            int compRes;
            if (cv1 != null && cv2 != null) {
                compRes = cv1.CompareTo(cv2);
            } else if (cv1 != null) {
                compRes = -1;
            } else if (cv2 != null) {
                compRes = 1;
            } else {
                compRes = 0;
            }
            return compRes;
        }
    }

    public sealed class GKListViewItems : IListViewItems
    {
        private readonly GKListView fListView;

        public BSDListItem this[int index]
        {
            get { return (IListItem)fListView.Items[index]; }
        }

        public int Count
        {
            get { return fListView.Items.Count; }
        }

        public GKListViewItems(GKListView listView)
        {
            fListView = listView;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GKListView : ListView, IListView
    {
        private class LVColumnSorter : IComparer
        {
            private readonly GKListView fOwner;

            public LVColumnSorter(GKListView owner)
            {
                fOwner = owner;
            }

            public int Compare(object x, object y)
            {
                int result = 0;

                int sortColumn = fOwner.fSortColumn;
                BSDSortOrder sortOrder = fOwner.fSortOrder;

                if (sortOrder != BSDSortOrder.None && sortColumn >= 0) {
                    ListViewItem item1 = (ListViewItem)x;
                    ListViewItem item2 = (ListViewItem)y;

                    if (sortColumn == 0) {
                        if (item1 is IComparable && item2 is IComparable) {
                            IComparable eitem1 = (IComparable)x;
                            IComparable eitem2 = (IComparable)y;
                            result = eitem1.CompareTo(eitem2);
                        } else {
                            result = GKUtils.StrCompareEx(item1.Text, item2.Text);
                        }
                    } else if (sortColumn < item1.SubItems.Count && sortColumn < item2.SubItems.Count) {
                        ListViewItem.ListViewSubItem subitem1 = item1.SubItems[sortColumn];
                        ListViewItem.ListViewSubItem subitem2 = item2.SubItems[sortColumn];

                        if (subitem1 is IComparable && subitem2 is IComparable) {
                            IComparable sub1 = (IComparable)subitem1;
                            IComparable sub2 = (IComparable)subitem2;
                            result = sub1.CompareTo(sub2);
                        } else {
                            result = GKUtils.StrCompareEx(subitem1.Text, subitem2.Text);
                        }
                    }

                    if (sortOrder == BSDSortOrder.Descending) {
                        result = -result;
                    }
                }

                return result;
            }
        }

        private readonly LVColumnSorter fColumnSorter;
        private readonly GKListViewItems fItemsAccessor;

        private readonly ListViewAppearance fAppearance;
        private GKListItem[] fCache;
        private int fCacheFirstItem;
        private IListSource fListMan;
        private int fSortColumn;
        private BSDSortOrder fSortOrder;
        private int fUpdateCount;


        public ListViewAppearance Appearance
        {
            get { return fAppearance; }
        }

        IListViewItems IListView.Items
        {
            get { return fItemsAccessor; }
        }

        public IListSource ListMan
        {
            get {
                return fListMan;
            }
            set {
                if (fListMan != value) {
                    fListMan = value;

                    if (fListMan != null) {
                        VirtualMode = true;
                        fSortColumn = 0;
                        fSortOrder = BSDSortOrder.Ascending;
                    } else {
                        VirtualMode = false;
                    }
                }
            }
        }

        public int SelectedIndex
        {
            get {
                return Items.IndexOf(GetSelectedItem());
            }
            set {
                SelectItem(value);
            }
        }

        public int SortColumn
        {
            get { return fSortColumn; }
            set { fSortColumn = value; }
        }

        public BSDSortOrder SortOrder
        {
            get { return fSortOrder; }
            set { fSortOrder = value; }
        }


        public event EventHandler ItemsUpdated;


        public GKListView()
        {
            fAppearance = new ListViewAppearance(this);

            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            // Enable the OnNotifyMessage event so we get a chance to filter out
            // Windows messages before they get to the form's WndProc
            SetStyle(ControlStyles.EnableNotifyMessage, true);

            OwnerDraw = true;
            HideSelection = false;
            LabelEdit = false;
            FullRowSelect = true;
            View = View.Details;

            fSortColumn = 0;
            fSortOrder = BSDSortOrder.None;
            fColumnSorter = new LVColumnSorter(this);
            fItemsAccessor = new GKListViewItems(this);

            ListViewItemSorter = fColumnSorter;

            fListMan = null;
        }

        public void Activate()
        {
            Select();
        }

        public new void BeginUpdate()
        {
            if (fUpdateCount == 0) {
                #if !MONO
                ListViewItemSorter = null;
                #endif
                base.BeginUpdate();
            }
            fUpdateCount++;
        }

        public new void EndUpdate()
        {
            fUpdateCount--;
            if (fUpdateCount == 0) {
                base.EndUpdate();
                #if !MONO
                ListViewItemSorter = fColumnSorter;
                #endif
            }
        }

        protected BSDSortOrder GetColumnSortOrder(int columnIndex)
        {
            return (fSortColumn == columnIndex) ? fSortOrder : BSDSortOrder.None;
        }

        public void SetSortColumn(int sortColumn, bool checkOrder = true)
        {
            int prevColumn = fSortColumn;
            if (prevColumn == sortColumn && checkOrder) {
                BSDSortOrder prevOrder = GetColumnSortOrder(sortColumn);
                fSortOrder = (prevOrder == BSDSortOrder.Ascending) ? BSDSortOrder.Descending : BSDSortOrder.Ascending;
            }

            fSortColumn = sortColumn;
            SortContents(true);
        }

        public void Sort(int sortColumn, BSDSortOrder sortOrder)
        {
            fSortOrder = sortOrder;
            SetSortColumn(sortColumn, false);
        }

        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            SetSortColumn(e.Column);

            // we use Refresh() because only Invalidate() isn't update header's area
            Refresh();

            base.OnColumnClick(e);
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = false;

            using (var sf = new StringFormat()) {
                Graphics gfx = e.Graphics;
                Rectangle rt = e.Bounds;

                if ((e.State & ListViewItemStates.Selected) == ListViewItemStates.Selected) {
                    DrawHeaderBackground(gfx, fAppearance.HeaderPressed, rt);
                } else {
                    DrawHeaderBackground(gfx, fAppearance.Header, rt);
                }

                switch (e.Header.TextAlign) {
                    case HorizontalAlignment.Left:
                        sf.Alignment = StringAlignment.Near;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                }

                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;

                int w = TextRenderer.MeasureText(" ", Font).Width;
                rt.Inflate(-(w / 5), 0);

                using (var brush = new SolidBrush(fAppearance.HeaderText)) {
                    gfx.DrawString(e.Header.Text, Font, brush, rt, sf);
                }

                switch (GetColumnSortOrder(e.ColumnIndex)) {
                    case BSDSortOrder.Ascending:
                        DrawSortArrow(gfx, rt, "▲");
                        break;
                    case BSDSortOrder.Descending:
                        DrawSortArrow(gfx, rt, "▼");
                        break;
                }
            }

            base.OnDrawColumnHeader(e);
        }

        private void DrawSortArrow(Graphics gfx, Rectangle rt, string arrow)
        {
            using (var fnt = new Font(Font.FontFamily, Font.SizeInPoints * 0.6f, FontStyle.Regular)) {
                float aw = gfx.MeasureString(arrow, fnt).Width;
                float x = rt.Left + (rt.Width - aw) / 2.0f;
                gfx.TextRenderingHint = TextRenderingHint.AntiAlias;
                gfx.DrawString(arrow, fnt, Brushes.Black, x, rt.Top);
            }
        }

        private static void DrawHeaderBackground(Graphics g, Color backColor, Rectangle bounds, bool classic3d = false)
        {
            using (Brush brush = new SolidBrush(backColor)) {
                g.FillRectangle(brush, bounds);
            }

            Rectangle rect = bounds;
            if (classic3d) {
                rect.Width--;
                rect.Height--;
                g.DrawRectangle(SystemPens.ControlDarkDark, rect);
                rect.Width--;
                rect.Height--;
                g.DrawLine(SystemPens.ControlLightLight, rect.X, rect.Y, rect.Right, rect.Y);
                g.DrawLine(SystemPens.ControlLightLight, rect.X, rect.Y, rect.X, rect.Bottom);
                g.DrawLine(SystemPens.ControlDark, rect.X + 1, rect.Bottom, rect.Right, rect.Bottom);
                g.DrawLine(SystemPens.ControlDark, rect.Right, rect.Y + 1, rect.Right, rect.Bottom);
            } else {
                rect.Width--;
                rect.Height--;
                g.DrawLine(SystemPens.ControlDark, rect.Right, rect.Y + 1, rect.Right, rect.Bottom - 1);
            }
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawItem(e);
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawSubItem(e);
        }

        #region Virtual mode with ListSource

        private GKListItem GetVirtualItem(int itemIndex)
        {
            object rowData = fListMan.GetContentItem(itemIndex);
            if (rowData == null) {
                return null;
            } else {
                object[] columnValues = fListMan.GetItemData(rowData);
                if (columnValues == null)
                    return null;

                var item = new GKListItem(columnValues[0], rowData);
                for (int i = 1, num = columnValues.Length; i < num; i++)
                    item.SubItems.Add(new GKListSubItem(columnValues[i]));

                var backColor = fListMan.GetBackgroundColor(itemIndex, rowData);
                if (backColor != null) {
                    item.SetBackColor(backColor);
                }

                return item;
            }
        }

        protected override void OnCacheVirtualItems(CacheVirtualItemsEventArgs e)
        {
            // Only recreate the cache if we need to.
            if (fCache != null && e.StartIndex >= fCacheFirstItem && e.EndIndex <= fCacheFirstItem + fCache.Length) return;

            fCacheFirstItem = e.StartIndex;
            int length = e.EndIndex - e.StartIndex + 1;

            if (fCache == null || fCache.Length != length) {
                fCache = new GKListItem[length];
            }

            for (int i = 0; i < length; i++) {
                fCache[i] = GetVirtualItem(fCacheFirstItem + i);
            }
        }

        protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {
            // If we have the item cached, return it. Otherwise, recreate it.
            if (fCache != null && e.ItemIndex >= fCacheFirstItem && e.ItemIndex < fCacheFirstItem + fCache.Length) {
                e.Item = fCache[e.ItemIndex - fCacheFirstItem];
            } else {
                e.Item = GetVirtualItem(e.ItemIndex);
            }
        }

        public void ResetCache()
        {
            fCache = null;
        }

        protected override void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
        {
            if (fListMan != null && fUpdateCount == 0) {
                fListMan.ChangeColumnWidth(e.ColumnIndex, Columns[e.ColumnIndex].Width);
            }

            Invalidate();

            base.OnColumnWidthChanged(e);
        }

        private void SortContents(bool restoreSelected)
        {
            if (fListMan != null) {
                object rec = (restoreSelected) ? GetSelectedData() : null;

                fListMan.SortContents(fSortColumn, fSortOrder == BSDSortOrder.Ascending);
                ResetCache();

                if (restoreSelected) SelectItem(rec);
            } else {
                Sort();
            }
        }

        public void SortModelColumn(int columnId)
        {
            if (fListMan != null && !AppHost.TEST_MODE) {
                int sortColumn = fListMan.GetColumnIndex(columnId);
                if (sortColumn != -1) {
                    SetSortColumn(sortColumn, false);
                }
            }
        }

        private void DoItemsUpdated()
        {
            var eventHandler = ItemsUpdated;
            if (eventHandler != null) eventHandler(this, new EventArgs());
        }

        public void UpdateContents(bool columnsChanged = false)
        {
            if (fListMan == null) return;

            try {
                object tempRec = GetSelectedData();
                BeginUpdate();
                try {
                    if (columnsChanged || Columns.Count == 0) {
                        fListMan.UpdateColumns(this);
                    }

                    fListMan.UpdateContents();
                    SortContents(false);
                    VirtualListSize = fListMan.FilteredCount;

                    #if MONO
                    if (fListMan.FilteredCount != 0) {
                        TopItem = Items[0];
                    }
                    #endif

                    ResizeColumns();
                } finally {
                    EndUpdate();
                    if (tempRec != null) SelectItem(tempRec);

                    DoItemsUpdated();
                }
            } catch (Exception ex) {
                Logger.WriteError("GKListView.UpdateContents()", ex);
            }
        }

        public void DeleteRecord(object data)
        {
            // crash protection: when you delete records from the diagrams,
            // between the actual deleting a record and updating the list
            // may take a few requests to update the list's items which does not already exist
            if (fListMan != null && fListMan.DeleteItem(data)) {
                VirtualListSize = fListMan.FilteredCount;
            }
        }

        #endregion

        #region Public methods

        public new void Clear()
        {
            // identical clearing of columns and items
            base.Clear();
        }

        public void ClearColumns()
        {
            Columns.Clear();
        }

        public void AddCheckedColumn(string caption, int width, bool autoSize = false)
        {
            CheckBoxes = true;
            AddColumn(caption, width, autoSize);
        }

        public void AddColumn(string caption, int width, bool autoSize = false)
        {
            if (autoSize) width = -1;
            Columns.Add(caption, width, HorizontalAlignment.Left);
        }

        public void AddColumn(string caption, int width, bool autoSize, BSDTypes.HorizontalAlignment textAlign)
        {
            if (autoSize) width = -1;
            Columns.Add(caption, width, (HorizontalAlignment)textAlign);
        }

        public void SetColumnCaption(int index, string caption)
        {
            Columns[index].Text = caption;
        }

        public void ResizeColumn(int columnIndex)
        {
            try {
                if (columnIndex >= 0 && Items.Count > 0) {
                    AutoResizeColumn(columnIndex, ColumnHeaderAutoResizeStyle.ColumnContent);

                    if (Columns[columnIndex].Width < 20) {
                        AutoResizeColumn(columnIndex, ColumnHeaderAutoResizeStyle.HeaderSize);
                    }
                }
            } catch (Exception ex) {
                Logger.WriteError("GKListView.ResizeColumn()", ex);
            }
        }

        public void ResizeColumns()
        {
            if (fListMan == null) return;

            for (int i = 0; i < Columns.Count; i++) {
                if (fListMan.IsColumnAutosize(i)) {
                    ResizeColumn(i);
                }
            }
        }

        public void ClearItems()
        {
            Items.Clear();
        }

        public BSDListItem AddItem(object rowData, bool isChecked, Color backColor, params object[] columnValues)
        {
            var item = AddItem(rowData, columnValues);
            if (CheckBoxes) {
                ((GKListItem)item).Checked = isChecked;
            }
            ((GKListItem)item).BackColor = backColor;
            return item;
        }

        public BSDListItem AddItem(object rowData, bool isChecked, params object[] columnValues)
        {
            var item = AddItem(rowData, columnValues);
            if (CheckBoxes) {
                ((GKListItem)item).Checked = isChecked;
            }
            return item;
        }

        public BSDListItem AddItem(object rowData, params object[] columnValues)
        {
            var result = new GKListItem(columnValues[0], rowData);
            Items.Add(result);

            int num = columnValues.Length;
            if (num > 1) {
                for (int i = 1; i < num; i++) {
                    object val = columnValues[i];
                    result.SubItems.Add(new GKListSubItem(val));
                }
            }

            return result;
        }

        public IList<object> GetSelectedItems()
        {
            try {
                var result = new List<object>();

                if (!VirtualMode) {
                    int num = SelectedItems.Count;
                    for (int i = 0; i < num; i++) {
                        var lvItem = SelectedItems[i] as GKListItem;
                        result.Add(lvItem.Tag);
                    }
                } else {
                    int num = SelectedIndices.Count;
                    for (int i = 0; i < num; i++) {
                        int index = SelectedIndices[i];
                        result.Add(fListMan.GetContentItem(index));
                    }
                }

                return result;
            } catch (Exception ex) {
                Logger.WriteError("GKListView.GetSelectedItems()", ex);
                return null;
            }
        }

        public GKListItem GetSelectedItem()
        {
            GKListItem result;

            if (SelectedItems.Count <= 0) {
                result = null;
            } else {
                result = (SelectedItems[0] as GKListItem);
            }

            return result;
        }

        public object GetSelectedData()
        {
            try {
                object result = null;

                if (!VirtualMode) {
                    GKListItem item = GetSelectedItem();
                    if (item != null) result = item.Tag;
                } else {
                    if (SelectedIndices.Count > 0) {
                        int index = SelectedIndices[0];
                        result = fListMan.GetContentItem(index);
                    }
                }

                return result;
            } catch (Exception ex) {
                Logger.WriteError("GKListView.GetSelectedData()", ex);
                return null;
            }
        }

        private void SelectItem(int index, GKListItem item)
        {
            if (item != null) {
                SelectedIndices.Clear();
                item.Selected = true;

                // in Mono `item.EnsureVisible()` doesn't work
                EnsureVisible(index);
            }
        }

        public void SelectItem(int index)
        {
            if (index == -1) {
                index = Items.Count - 1;
            }

            if (index >= 0 && index < Items.Count) {
                var item = (GKListItem)Items[index];
                SelectItem(index, item);
            }
        }

        public void SelectItem(object rowData)
        {
            if (rowData == null)
                return;

            try {
                if (fListMan != null) {
                    // "virtual" mode
                    int idx = fListMan.IndexOfItem(rowData);
                    if (idx >= 0) {
                        var item = (GKListItem)Items[idx];
                        SelectItem(idx, item);
                    }
                } else {
                    int num = Items.Count;
                    for (int i = 0; i < num; i++) {
                        var item = (GKListItem)Items[i];
                        if (item.Tag == rowData) {
                            SelectItem(i, item);
                            return;
                        }
                    }
                }
            } catch (Exception ex) {
                Logger.WriteError("GKListView.SelectItem()", ex);
            }
        }

        #endregion
    }


    public class ListViewAppearance
    {
        private readonly ListView fOwner;
        private Color fBackColor;
        private Color fHeader;
        private Color fHeaderPressed;
        private Color fHeaderText;
        private Color fInterlaced;
        private Color fItemSelected;

        public Color BackColor
        {
            get {
                return fBackColor;
            }
            set {
                if (fBackColor != value) {
                    fBackColor = value;
                    fOwner.Invalidate();
                }
            }
        }

        public Color Header
        {
            get {
                return fHeader;
            }
            set {
                if (fHeader != value) {
                    fHeader = value;
                    fOwner.Invalidate();
                }
            }
        }

        public Color HeaderPressed
        {
            get {
                return fHeaderPressed;
            }
            set {
                if (fHeaderPressed != value) {
                    fHeaderPressed = value;
                    fOwner.Invalidate();
                }
            }
        }

        public Color HeaderText
        {
            get {
                return fHeaderText;
            }
            set {
                if (fHeaderText != value) {
                    fHeaderText = value;
                    fOwner.Invalidate();
                }
            }
        }

        public Color Interlaced
        {
            get {
                return fInterlaced;
            }
            set {
                if (fInterlaced != value) {
                    fInterlaced = value;
                    fOwner.Invalidate();
                }
            }
        }

        public Color ItemSelected
        {
            get {
                return fItemSelected;
            }
            set {
                if (fItemSelected != value) {
                    fItemSelected = value;
                    fOwner.Invalidate();
                }
            }
        }

        public ListViewAppearance(ListView owner)
        {
            fOwner = owner;
            Reset(false);
        }

        public void Reset(bool invalidate = true)
        {
            fBackColor = SystemColors.Control;
            fHeader = SystemColors.Control;
            fHeaderPressed = Color.FromArgb(188, 220, 244); // BCDCF4;
            fHeaderText = SystemColors.ControlText;
            fInterlaced = SystemColors.ControlDark;
            fItemSelected = SystemColors.Highlight;

            if (invalidate)
                fOwner.Invalidate();
        }
    }
}
