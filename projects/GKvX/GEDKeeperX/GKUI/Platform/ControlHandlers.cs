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

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BSLib;
using GKCore.Design;
using GKCore.Design.Controls;
using GKCore.Design.Graphics;
using GKUI.Components;
using Xamarin.Forms;
using XFIKCheckBox = Plugin.InputKit.Shared.Controls.CheckBox;
using XFRadioButton = Xamarin.Forms.RadioButton;

namespace GKUI.Platform
{
    public abstract class BaseControlHandler<T, TThis> : ControlHandler<T, TThis>, IBaseControl
        where T : VisualElement
        where TThis : ControlHandler<T, TThis>
    {
        protected BaseControlHandler(T control) : base(control)
        {
        }

        public bool Enabled
        {
            get { return Control.IsEnabled; }
            set { Control.IsEnabled = value; }
        }

        public void Activate()
        {
            Control.Focus();
        }
    }


    public sealed class LabelHandler : BaseControlHandler<Label, LabelHandler>, ILabel
    {
        public LabelHandler(Label control) : base(control)
        {
        }

        public IColor BackColor
        {
            get { return new ColorHandler(Control.BackgroundColor); }
            set { Control.BackgroundColor = ((ColorHandler)value).Handle; }
        }

        public string Text
        {
            get { return Control.Text; }
            set { Control.Text = value; }
        }
    }

    public sealed class ButtonHandler : BaseControlHandler<Button, ButtonHandler>, IButton
    {
        public ButtonHandler(Button control) : base(control)
        {
        }

        public string Text
        {
            get { return Control.Text; }
            set { Control.Text = value; }
        }
    }

    public sealed class CheckBoxHandler : BaseControlHandler<XFIKCheckBox, CheckBoxHandler>, ICheckBox
    {
        public CheckBoxHandler(XFIKCheckBox control) : base(control)
        {
        }

        public bool Checked
        {
            get { return Control.IsChecked; }
            set { Control.IsChecked = value; }
        }

        public string Text
        {
            get { return Control.Text; }
            set { Control.Text = value; }
        }
    }

    public sealed class RadioButtonHandler : BaseControlHandler<XFRadioButton, RadioButtonHandler>, IRadioButton
    {
        public RadioButtonHandler(XFRadioButton control) : base(control)
        {
        }

        public bool Checked
        {
            get { return Control.IsChecked; }
            set { Control.IsChecked = value; }
        }

        public string Text
        {
            get { return (string)Control.Content; }
            set { Control.Content = value; }
        }
    }

    public sealed class ComboBoxHandler : BaseControlHandler<XFComboBox, ComboBoxHandler>, IComboBox
    {
        private readonly IList fItems;

        public ComboBoxHandler(XFComboBox control) : base(control)
        {
            fItems = new ObservableCollection<string>();
            control.ItemsSource = fItems;
        }

        public new bool Enabled
        {
            get { return Control.IsEnabled; }
            set {
                Control.IsEnabled = value;
                //Control.BackgroundColor = (value) ? SystemColors.WindowBackground : SystemColors.Control;
            }
        }

        public IList Items
        {
            get {
                return null;
            }
        }

        public bool ReadOnly
        {
            get { return false; /*Control.ReadOnly*/; }
            set { /*Control.ReadOnly = value;*/ }
        }

        public int SelectedIndex
        {
            get { return 0/*Control.SelectedIndex*/; }
            set { /*Control.SelectedIndex = value;*/ }
        }

        public object SelectedItem
        {
            get { return Control.SelectedItem; }
            set { Control.SelectedItem = value; }
        }

        public bool Sorted
        {
            get { return false; }
            set {
                if (value) {
                    Sort();
                }
            }
        }

        /*public object SelectedTag
        {
            get {
                return ((GKComboItem)Control.SelectedItem).Tag;
            }
            set {
                var ctl = Control;
                foreach (object item in ctl.Items) {
                    GKComboItem comboItem = (GKComboItem)item;
                    if (comboItem.Tag == value) {
                        ctl.SelectedItem = item;
                        return;
                    }
                }
                ctl.SelectedIndex = 0;
            }
        }*/

        public string Text
        {
            get { return /*Control.Text*/string.Empty; }
            set { /*Control.Text = value;*/ }
        }

        public void Add(object item)
        {
            //Control.ItemsSource = null;
            //Control.Items.Add((string)item);
            fItems.Add(item.ToString());
            //Control.ItemsSource = fItems;
        }

        public void AddItem<T>(string caption, T tag, IImage image = null)
        {
            //Control.Items.Add(new GKComboItem<T>(caption, tag, image));
            //Control.Items.Add(caption);
        }

        public void AddItem(string caption, object tag, IImage image = null)
        {
            //Control.Items.Add(new GKComboItem(caption, tag, image));
            //Control.Items.Add(caption);
        }

        public void AddItem<T>(string caption, T tag)
        {
            //Control.Items.Add(caption);
        }

        public void AddRange(object[] items, bool sorted = false)
        {
            //Control.Sorted = false;
            //Control.Items.AddRange(GKComboItem.Convert((string[])items));
            foreach (var itm in items)
            {
                //Control.Items.Add((string)itm);
            }
            //Control.Sorted = sorted;
        }

        public void AddRange(IEnumerable<object> items, bool sorted = false)
        {
            //Control.Sorted = false;
            //Control.Items.AddRange(GKComboItem.Convert((string[])items));
            foreach (var itm in items)
            {
                //Control.Items.Add((string)itm);
            }
            //Control.Sorted = sorted;
        }

        public void AddStrings(StringList strings)
        {
            int num = strings.Count;
            for (int i = 0; i < num; i++) {
                AddItem(strings[i], strings.GetObject(i));
            }
        }

        public void BeginUpdate()
        {
            //Control.BeginUpdate();
            //Control.ItemsSource = null;
        }

        public void Clear()
        {
            //Control.Items.Clear();
            //fItems.Clear();
        }

        public void EndUpdate()
        {
            //Control.ItemsSource = fItems;
            //Control.EndUpdate();
        }

        public void Sort()
        {
            //Control.SortItems();
        }

        public T GetSelectedTag<T>()
        {
            return default(T);
        }

        public void SetSelectedTag<T>(T tagValue, bool allowDefault = true)
        {
        }
    }

    public sealed class TextBoxHandler : BaseControlHandler<Entry, TextBoxHandler>, ITextBox
    {
        public TextBoxHandler(Entry control) : base(control)
        {
        }

        public new bool Enabled
        {
            get { return Control.IsEnabled; }
            set {
                Control.IsEnabled = value;
                SetBackColor();
            }
        }

        public string[] Lines
        {
            get { return /*UIHelper.Convert(Control.Text)*/ null; }
            set { } // TODO
        }

        public bool ReadOnly
        {
            get { return Control.IsReadOnly; }
            set {
                Control.IsReadOnly = value;
                SetBackColor();
            }
        }

        public string SelectedText
        {
            get { return /*Control.SelectedText*/ string.Empty; }
            set { /*Control.SelectedText = value;*/ }
        }

        public string Text
        {
            get { return Control.Text; }
            set { Control.Text = value; }
        }

        public void AppendText(string text)
        {
            //Control.Append(text, true);
        }

        public void Clear()
        {
            Control.Text = string.Empty;
        }

        private void SetBackColor()
        {
            //Control.BackgroundColor = (!Control.ReadOnly && Enabled) ? SystemColors.WindowBackground : SystemColors.Control;
        }

        public void Copy()
        {
            //UIHelper.SetClipboardText(Control.SelectedText);
        }

        public void SelectAll()
        {
            //Control.SelectAll();
        }
    }

    /*public sealed class TextAreaHandler : BaseControlHandler<Editor, TextAreaHandler>, ITextBox
    {
        public TextAreaHandler(Editor control) : base(control)
        {
        }

        public new bool Enabled
        {
            get { return Control.IsEnabled; }
            set {
                Control.IsEnabled = value;
                SetBackColor();
            }
        }

        public string[] Lines
        {
            get { return UIHelper.Convert(Control.Text); }
            set { } // TODO
        }

        public bool ReadOnly
        {
            get { return Control.ReadOnly; }
            set {
                Control.ReadOnly = value;
                SetBackColor();
            }
        }

        public string SelectedText
        {
            get { return Control.SelectedText; }
            set { Control.SelectedText = value; }
        }

        public string Text
        {
            get { return Control.Text; }
            set { Control.Text = value; }
        }

        public void AppendText(string text)
        {
            Control.Append(text, true);
        }

        public void Clear()
        {
            Control.Text = string.Empty;
        }

        private void SetBackColor()
        {
            Control.BackgroundColor = (!Control.ReadOnly && Enabled) ? SystemColors.WindowBackground : SystemColors.Control;
        }

        public void Copy()
        {
            UIHelper.SetClipboardText(Control.SelectedText);
        }

        public void SelectAll()
        {
            Control.SelectAll();
        }
    }*/

    /*public sealed class MaskedTextBoxHandler : BaseControlHandler<Entry, MaskedTextBoxHandler>, ITextBox
    {
        public MaskedTextBoxHandler(Entry control) : base(control)
        {
        }

        public new bool Enabled
        {
            get { return Control.IsEnabled; }
            set {
                Control.IsEnabled = value;
                //Control.BackgroundColor = (value) ? SystemColors.WindowBackground : SystemColors.Control;
            }
        }

        public string[] Lines
        {
            get { return UIHelper.Convert(Control.Text); }
            set {  } // TODO
        }

        public bool ReadOnly
        {
            get { return Control.ReadOnly; }
            set { Control.ReadOnly = value; }
        }

        public string SelectedText
        {
            get { return Control.SelectedText; }
            set { Control.SelectedText = value; }
        }

        public string Text
        {
            get { return Control.Text; }
            set { Control.Text = value; }
        }

        public void AppendText(string text)
        {
            //Control.Append(text, true);
        }

        public void Clear()
        {
            Control.Text = string.Empty;
        }

        public void Copy()
        {
            UIHelper.SetClipboardText(Control.SelectedText);
        }

        public void SelectAll()
        {
            Control.SelectAll();
        }
    }*/

    /*public sealed class NumericBoxHandler : BaseControlHandler<Stepper, NumericBoxHandler>, INumericBoxHandler
    {
        public NumericBoxHandler(Stepper control) : base(control)
        {
        }

        public bool ReadOnly
        {
            get { return Control.ReadOnly; }
            set { Control.ReadOnly = value; }
        }

        public string Text
        {
            get { return Value.ToString(); }
            set { } // TODO
        }

        public double Value
        {
            get { return Control.Value; }
            set { Control.Value = value; }
        }
    }*/

    /*public sealed class TreeViewHandler : BaseControlHandler<TreeView, TreeViewHandler>, ITreeViewHandler
    {
        private TreeItem fRootNode;

        public TreeViewHandler(TreeView control) : base(control)
        {
        }

        public ITVNode AddNode(ITVNode parent, string name, object tag)
        {
            var node = new GKTreeNode(name, tag);
            if (parent == null) {
                fRootNode.Children.Add(node);
            } else {
                ((GKTreeNode)parent).Children.Add(node);
            }
            return node;
        }

        public void BeginUpdate()
        {
            Control.DataStore = null;
            fRootNode = new TreeItem();
        }

        public void Clear()
        {
        }

        public void EndUpdate()
        {
            Control.DataStore = fRootNode;
            Control.RefreshData();
        }

        public void Expand(ITVNode node)
        {
            GKTreeNode treeNode = node as GKTreeNode;
            if (treeNode != null) {
                treeNode.Expanded = true;
            }
        }
    }*/

    /*public sealed class ProgressBarHandler : BaseControlHandler<ProgressBar, ProgressBarHandler>, IProgressBarHandler
    {
        public ProgressBarHandler(ProgressBar control) : base(control)
        {
        }

        public int Minimum
        {
            get { return Control.MinValue; }
            set { Control.MinValue = value; }
        }

        public int Maximum
        {
            get { return Control.MaxValue; }
            set { Control.MaxValue = value; }
        }

        public int Value
        {
            get { return Control.Value; }
            set { Control.Value = value; }
        }

        public void Increment(int value)
        {
            Control.Value += value;
        }
    }*/

    /*public sealed class LogChartHandler : BaseControlHandler<LogChart, LogChartHandler>, ILogChart
    {
        public LogChartHandler(LogChart control) : base(control)
        {
        }

        public void AddFragment(int val)
        {
            Control.AddFragment(val);
        }

        public void Clear()
        {
            Control.Clear();
        }
    }*/

    /*public sealed class TabControlHandler : BaseControlHandler<TabControl, TabControlHandler>, ITabControl
    {
        public TabControlHandler(TabControl control) : base(control)
        {
        }

        public int SelectedIndex
        {
            get { return Control.SelectedIndex; }
            set { Control.SelectedIndex = value; }
        }
    }*/

    /*public sealed class MenuItemHandler : ControlHandler<ButtonMenuItem, MenuItemHandler>, IMenuItem
    {
        public MenuItemHandler(ButtonMenuItem control) : base(control)
        {
        }

        public bool Checked
        {
            get { return false; }
            set { }
        }

        public bool Enabled
        {
            get { return Control.Enabled; }
            set { Control.Enabled = value; }
        }

        public object Tag
        {
            get { return Control.Tag; }
            set { Control.Tag = value; }
        }

        public int ItemsCount
        {
            get { return Control.Items.Count; }
        }

        public IMenuItem AddItem(string text, object tag, IImage image, ItemAction action)
        {
            var item = new MenuItemEx(text, tag, image, action);
            Control.Items.Add(item);
            return item;
        }

        public void ClearItems()
        {
            Control.Items.Clear();
        }
    }*/
}
