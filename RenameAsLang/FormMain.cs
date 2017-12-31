using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Ambiesoft;
using System.IO;
namespace RenameAsLang
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            string folder = CppUtils.GetSelectedFolder(this, Properties.Resources.CHOOSE_FOLDER);
            if (string.IsNullOrEmpty(folder))
                return;

            txtFolder.Text = folder;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cmbEncodings.SelectedItem == null)
            {
                CppUtils.Alert(Properties.Resources.SELECT_ENCODING);
                return;
            }
            lvResult.Items.Clear();

            var ei = (ComboItemInfo)cmbEncodings.SelectedItem;
            Encoding encoding = ei.EncInfo.GetEncoding();

            DirectoryInfo di = new DirectoryInfo(txtFolder.Text);

            Dictionary<char, string> charmap = new Dictionary<char, string>();
            FileInfo[] fis = di.GetFiles("*.*", SearchOption.AllDirectories);
            if(fis.Length==0)
            {
                CppUtils.Info(Properties.Resources.NO_FILES);
                return;
            }
            foreach (FileInfo fi in fis)
            {
                string resultName = string.Empty;
                foreach (char c in fi.Name)
                {
                    byte[] b = encoding.GetBytes(c.ToString());
                    string ret = encoding.GetString(b);
                    if (c.ToString() == ret)
                    {
                        resultName += c;
                    }
                    else
                    {
                        string newchar = string.Empty;
                        if (charmap.ContainsKey(c))
                        {
                            newchar = charmap[c];
                        }
                        else
                        {
                            using (FormNewchar form = new FormNewchar())
                            {
                                form.txtOldchar.Text = c.ToString();
                                if (DialogResult.OK == form.ShowDialog() && !string.IsNullOrEmpty(form.txtNewchar.Text))
                                {
                                    newchar = form.txtNewchar.Text;
                                }
                                else
                                {
                                    newchar = System.Web.HttpUtility.UrlEncode(c.ToString(), Encoding.UTF8).ToUpper();
                                }
                                charmap[c] = newchar;
                            }
                        }
                        resultName += newchar;
                    }
                }
                if (fi.Name != resultName)
                {
                    ListViewItem lvItem = new ListViewItem();
                    lvItem.Text = fi.Name;
                    lvItem.Name = "old";

                    ListViewItem.ListViewSubItem subitem = new ListViewItem.ListViewSubItem();
                    subitem.Text = resultName;
                    subitem.Name = "new";
                    lvItem.SubItems.Add(subitem);

                    ListViewItem.ListViewSubItem subitemFolder = new ListViewItem.ListViewSubItem();
                    subitemFolder.Text = fi.DirectoryName;
                    subitemFolder.Name = "folder";
                    lvItem.SubItems.Add(subitemFolder);

                    lvResult.Items.Add(lvItem);
                }
            }

            if(lvResult.Items.Count==0)
            {
                CppUtils.Info(string.Format(Properties.Resources.EVERY_CHARACTER_ENCODABLE, ei.EncInfo.DisplayName));
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                cmbEncodings.Items.Add(new ComboItemInfo(ei));
            }
        }

        private void openInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvResult.SelectedItems.Count == 0)
                return;
            var item = lvResult.SelectedItems[0];

            string fullpath = Path.Combine(txtFolder.Text, item.Text);
            if(!File.Exists(fullpath))
            {
                CppUtils.Alert(string.Format(Properties.Resources.FILE_NOT_FOUND, fullpath));
                return;
            }
            CppUtils.OpenFolder(this, fullpath);
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            List<ListViewItem> itemToDelete = new List<ListViewItem>();
            foreach(ListViewItem item in lvResult.Items)
            {
                string olds = item.SubItems["old"].Text;
                string news = item.SubItems["new"].Text;

                olds = Path.Combine(item.SubItems["folder"].Text, olds);
                news = Path.Combine(item.SubItems["folder"].Text, news);

                try
                {
                    File.Move(olds, news);
                    itemToDelete.Add(item);
                }
                catch(Exception ex)
                {
                    CppUtils.Alert(ex);
                }
            }
            foreach(ListViewItem item in itemToDelete)
            {
                lvResult.Items.Remove(item);
            }
        }

    }
}
