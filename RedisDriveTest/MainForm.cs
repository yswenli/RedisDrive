/*
* 描述： 详细描述类能干什么
* 创建人：Administrator
* 创建时间：2017/2/6 16:02:32
*/
/*
*修改人：Administrator
*修改时间：2017/2/6 16:02:32
*修改内容：xxxxxxx
*/


using CCWin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RedisDriveTest
{
    public partial class MainForm : Skin_Mac
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Worker.Init(this);
        }

        private void skinRadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Worker.SetDefaultValue(this);
        }

        private void skinRadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Worker.SetDefaultValue(this);
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {
            Worker.Work(this);
        }

        #region 菜单
        private void contextMenuStrip1_Opened(object sender, EventArgs e)
        {
            var txt = ((TextBox)((ContextMenuStrip)sender).SourceControl);

            selectAllToolStripMenuItem.Enabled = true;
            if (string.IsNullOrWhiteSpace(txt.Text))
                selectAllToolStripMenuItem.Enabled = false;

            copyToolStripMenuItem.Enabled = true;
            if (string.IsNullOrWhiteSpace(txt.SelectedText))
                copyToolStripMenuItem.Enabled = false;

            pasteToolStripMenuItem.Enabled = false;
            if (txt.Name == "textBox1")
                pasteToolStripMenuItem.Enabled = true;
        }
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var txt = ((TextBox)((ContextMenuStrip)(((ToolStripMenuItem)sender)).GetCurrentParent()).SourceControl);
            txt.SelectAll();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var txt = ((TextBox)((ContextMenuStrip)(((ToolStripMenuItem)sender)).GetCurrentParent()).SourceControl);
                if (!string.IsNullOrWhiteSpace(txt.SelectedText))
                    Clipboard.SetText(txt.SelectedText);
            }
            catch
            {

            }

        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var txt = ((TextBox)((ContextMenuStrip)(((ToolStripMenuItem)sender)).GetCurrentParent()).SourceControl);
                int index = txt.SelectionStart;
                txt.Text = txt.Text.Insert(index, Clipboard.GetText());
            }
            catch
            {

            }
        }

        #endregion

        #region 快捷键
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Modifiers == Keys.Control) && (e.KeyCode == Keys.A))
                ((TextBox)sender).SelectAll();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Modifiers == Keys.Control) && (e.KeyCode == Keys.A))
                ((TextBox)sender).SelectAll();
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Modifiers == Keys.Control) && (e.KeyCode == Keys.A))
                ((TextBox)sender).SelectAll();
        }
        #endregion
    }
}
