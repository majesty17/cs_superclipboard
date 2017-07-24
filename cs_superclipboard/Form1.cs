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

            Hotkey hotkey;
            hotkey = new Hotkey(this.Handle);
            Hotkey1 = hotkey.RegisterHotkey(System.Windows.Forms.Keys.F11, Hotkey.KeyFlags.MOD_CONTROL);   //定义快键(Ctrl + F2)
            hotkey.OnHotkey += new HotkeyEventHandler(OnHotkey);

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
            for (int i = 0; i < all_data.Count;i++)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = all_data[i];
                listView1.Items.Add(lvi);
            }
            columnHeader1.Width = 540;

            //
            //头条插入当前剪切板
            if (all_data.Count > 0)
                Clipboard.SetData("Text", all_data[0]);
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

        public void OnHotkey(int HotkeyID) 
        {
            if (HotkeyID == Hotkey1)
            {
                
            }
        }

    }
}
