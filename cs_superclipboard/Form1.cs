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
using System.Collections;
using System.Resources;
using System.Reflection;

namespace cs_superclipboard
{
    public partial class Form1 : Form
    {
        SoundPlayer player = new SoundPlayer();
        List<string> all_data = new List<string>();
        Help h = new Help();


        [DllImport("user32.dll")]
        private static extern void keybd_event(
            byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);  


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "超级剪切板 v0.1";

            timer1.Start();
            player.SoundLocation = @"C:\Windows\Media\Windows Notify Messaging.wav";
            player.Load();

            Hotkey hotkey;
            hotkey = new Hotkey(this.Handle);
            Hotkey1 = hotkey.RegisterHotkey(System.Windows.Forms.Keys.F2, Hotkey.KeyFlags.MOD_CONTROL);   //定义快键(Ctrl + F2)
            hotkey.OnHotkey += new HotkeyEventHandler(OnHotkey);


        }



        //定时器
        private void timer1_Tick(object sender, EventArgs e) {
            IDataObject data = Clipboard.GetDataObject();
            if (data.GetDataPresent(DataFormats.Text) == true) {
                string text = (string)data.GetData(DataFormats.Text);
                text = text.Trim();
                //如果剪切板内容不为空，则进一步判断
                if (text.Length > 0){
                    //如果不在，则直接插入
                    if (hasData(text) == false){
                        //all_data.Add(text); 插入到第一，而不是追加
                        all_data.Insert(0, text);
                        showData();
                        player.Play();
                    }
                    //如果在，并且不在第一位，则移动到第一位
                    if (hasData(text) == true && all_data[0].Equals(text) == false) {
                        int ind = all_data.IndexOf(text);
                        string f_text = all_data[0];
                        all_data[0] = text;
                        all_data[ind] = f_text;
                        showData();
                    }
                }
            }
        }

        //显示数据
        private void showData() {
            listView1.Items.Clear();
            for (int i = 0; i < all_data.Count;i++)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = all_data[i];
                lvi.SubItems.Add("" + (i + 1));
                listView1.Items.Add(lvi);
            }
            columnHeader1.Width = 500;

            //
            //头条插入当前剪切板&加黑
            if (all_data.Count > 0) {
                (listView1.Items[0]).ForeColor = Color.Red;
                Clipboard.SetData("Text", all_data[0]);
            }
            else
                Clipboard.Clear();
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
            if (e.KeyData == Keys.OemMinus) {
                if (all_data.Count > 1) {
                    string f_data = all_data[0];
                    all_data.RemoveAt(0);
                    all_data.Add(f_data);
                    showData();

                }
            }
            if (e.KeyData == Keys.Oemplus)
            {
                if (all_data.Count > 1)
                {
                    string f_data = all_data[all_data.Count-1];
                    all_data.RemoveAt(all_data.Count - 1);
                    all_data.Insert(0, f_data);
                    showData();

                }
            }
            if ((int)e.KeyData >= 49 && (int)e.KeyData <= 57) {
                int ind = (int)e.KeyData - 48-1;
                //MessageBox.Show("press:" + ind);
                if (all_data.Count > ind && ind>0) {
                    string a_data = all_data[ind];
                    string f_data=all_data[0];
                    all_data[0] = a_data;
                    all_data[ind] = f_data;
                    showData();
                }
            }
        }
        //双击提前
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1 || listView1.Items[0].Selected==true)
                return;
            string s_text = listView1.SelectedItems[0].Text;
            int ind = all_data.IndexOf(s_text);
            string f_text = all_data[0];

            all_data[0] = s_text;
            all_data[ind] = f_text;
            showData();
        }

        //双击通知栏图标
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
                this.Activate();
                this.ShowInTaskbar = true;
            }
        }

        //窗口大小变化
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }



        public delegate void HotkeyEventHandler(int HotKeyID);
        private int Hotkey1;
        public class Hotkey : System.Windows.Forms.IMessageFilter
        {
            Hashtable keyIDs = new Hashtable();
            IntPtr hWnd;

            public event HotkeyEventHandler OnHotkey;

            public enum KeyFlags
            {
                MOD_ALT = 0x1,
                MOD_CONTROL = 0x2,
                MOD_SHIFT = 0x4,
                MOD_WIN = 0x8
            }
            [DllImport("user32.dll")]
            public static extern UInt32 RegisterHotKey(IntPtr hWnd, UInt32 id, UInt32 fsModifiers, UInt32 vk);

            [DllImport("user32.dll")]
            public static extern UInt32 UnregisterHotKey(IntPtr hWnd, UInt32 id);

            [DllImport("kernel32.dll")]
            public static extern UInt32 GlobalAddAtom(String lpString);

            [DllImport("kernel32.dll")]
            public static extern UInt32 GlobalDeleteAtom(UInt32 nAtom);

            public Hotkey(IntPtr hWnd)
            {
                this.hWnd = hWnd;
                Application.AddMessageFilter(this);
            }

            public int RegisterHotkey(Keys Key, KeyFlags keyflags)
            {
                UInt32 hotkeyid = GlobalAddAtom(System.Guid.NewGuid().ToString());
                RegisterHotKey((IntPtr)hWnd, hotkeyid, (UInt32)keyflags, (UInt32)Key);
                keyIDs.Add(hotkeyid, hotkeyid);
                return (int)hotkeyid;
            }

            public void UnregisterHotkeys()
            {
                Application.RemoveMessageFilter(this);
                foreach (UInt32 key in keyIDs.Values)
                {
                    UnregisterHotKey(hWnd, key);
                    GlobalDeleteAtom(key);
                }
            }

            public bool PreFilterMessage(ref System.Windows.Forms.Message m)
            {
                if (m.Msg == 0x312)
                {
                    if (OnHotkey != null)
                    {
                        foreach (UInt32 key in keyIDs.Values)
                        {
                            if ((UInt32)m.WParam == key)
                            {
                                OnHotkey((int)m.WParam);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }
        //热键处理
        public void OnHotkey(int HotkeyID) 
        {
            if (HotkeyID == Hotkey1)
            {
                Input("hahaha");
                MessageBox.Show("asdfsdf");

            }
        }

        public static bool Input(string str)
        {
            foreach (char ch in str)
            {
                //模拟单击键，可规避连续输入键值时遗漏或延长Thread.Sleep(50);  
                //keybd_event(0x1, 0, 1, 0);
                keybd_event(0x42, 0, 1, 0);
            }
            return false;
        } 


        //帮助
        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            h.Show();
        }
        //退出
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //清除所有
        private void 清除所有ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            all_data.Clear();
            Clipboard.Clear();
            showData();
        }

        //是否最前
        private void 在最前ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mt = (ToolStripMenuItem)sender;
            if (mt.Checked==false)
            {
                this.TopMost = false;
                this.Opacity = 1.0;
            }
            else {
                this.TopMost = true;
                this.Opacity = 0.650;
            }
        }






    }
}
