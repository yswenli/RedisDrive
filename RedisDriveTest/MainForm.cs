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
    }
}
