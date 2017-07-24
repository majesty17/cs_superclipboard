using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Media;

namespace cs_superclipboard
{
    public partial class Form1 : Form
    {
        SoundPlayer player = new SoundPlayer();
        List<string> all_data = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            player.SoundLocation = @"C:\Windows\Media\Windows Notify Messaging.wav";
            player.Load();

        }



        //定时器
        private void timer1_Tick(object sender, EventArgs e) {
            IDataObject data = Clipboard.GetDataObject();
            if (data.GetDataPresent(DataFormats.Text) == true) {
                string text = (string)data.GetData(DataFormats.Text);
                text = text.Trim();
                if (text.Length > 0 && hasData(text)==false) {
                    all_data.Add(text);
                    showData();
                    player.Play();
                }
            }
        }

        //显示数据
        private void showData() {
            listView1.Items.Clear();
            for (int i = all_data.Count - 1; i >= 0; i--) {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = all_data[i];
                listView1.Items.Add(lvi);
            }
            columnHeader1.Width = 540;

        }

        //是否存在
        private bool hasData(string text)
        {
            for (int i = 0; i < all_data.Count; i++) {
                if (all_data[i].Equals(text))
                    return true;
            }
            return false;
        }
        
        //删除
        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            string text = (string)Clipboard.GetDataObject().GetData(DataFormats.Text);
            if (e.KeyData == Keys.Delete) {
                foreach (ListViewItem it in listView1.SelectedItems) {
                    all_data.Remove(it.Text);
                    if (text.Equals(it.Text) == true) {
                        Clipboard.Clear();
                    }
                }
                showData();
            }
        }
    }
}
