using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Desktop_Search
{
    public partial class Form1 : Form
    {
        public static bool running_command = false;
        private System.Windows.Forms.Timer _typingTimer;
        public static int mouse_over_datagrid;
        public DataTable dt = new DataTable();
        public Dictionary<string, string> path_url = new Dictionary<string, string>();
        public Form1()
        {
            InitializeComponent();

            /* Main Screen Position on Load */
            var myScreen = Screen.FromControl(this);
            var mySecondScreen = Screen.AllScreens.FirstOrDefault(s => !s.Equals(myScreen)) ?? myScreen;
            this.Left = mySecondScreen.Bounds.Left;
            this.Top = mySecondScreen.Bounds.Top;
            StartPosition = FormStartPosition.Manual;

            
            this.dataGridView1.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            this.dataGridView1.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
            this.dataGridView1.AdvancedCellBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;
            this.dataGridView1.AdvancedCellBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;

            this.Text = "Desktop Search" + String.Format(" Version {0}", aboutBox1.AssemblyVersion); ;

            /* Default Definations */
            dt.Columns.Add("Name");
            dt.Columns.Add("Path");
            dt.Columns.Add("ItemType");
            dt.Columns.Add("DateCreated");
            dt.Columns.Add("DateModified");
            dt.Columns.Add("DateAccessed");
            dt.Columns.Add("Size (MB)");
            dt.Columns.Add("Checked ?");

            Keyword_Text.Text = "Enter text here...";
            Extension_Text.Text = ".*";
            Keyword_Text.ForeColor = Color.Gray;
            notifyIcon1.Icon = this.Icon;
        }

       
        public void RemoveText(object sender, EventArgs e)
        {
            Keyword_Text.Text = "";

        }
        public void RemoveText2(object sender, EventArgs e)
        {
            Extension_Text.Text = "";
        }

        public void AddDefaultText(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Keyword_Text.Text))
            {
                Keyword_Text.Text = "Enter text here...";
            }
        }
        public void AddDefaultText2(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Extension_Text.Text))
                Extension_Text.Text = ".*";
        }
        private void Search_Click(object sender, EventArgs e)
        {
            if (Regex.Replace(Keyword_Text.Text, @"\s+", "").Length > 1)  // Remove white spaces on keyword_text and checks the text lenght
            {
                dt.Rows.Clear();
                dataGridView1.Visible = false;
                dataGridView1.Refresh();
                string keyword = Keyword_Text.Text;
                string connectionString = "Provider=Search.CollatorDSO;Extended Properties=\"Application=Windows\"";

                string query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                    "WHERE scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword); //  + " AND Contains(System.ItemType, '.pdf')"  ;


                if (FullTS_checkBox.Checked == true)
                {
                    query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex WHERE " + String.Format(@"CONTAINS(*,'""{0}""',1033)", keyword) + " ORDER BY System.FileName ASC"; //+ @" AND (System.FileName LIKE '%.doc' OR System.FileName LIKE '%.txt') AND Contains(System.Kind, 'document') ORDER BY System.FileName ASC";
                }

                else if (OnlyEmails_checkBox.Checked == true)
                {
                    query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                     "WHERE scope ='mapi16:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword);
                }

                else
                {

                    int c_box_count = FileType_checkedListBox.CheckedItems.Count;
                    List<string> c_items = FileType_checkedListBox.CheckedItems.Cast<string>().ToList();


                    switch (c_box_count)
                    {
                        case 0:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword); 
                            break;
                        case 1:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE (System.FileName LIKE '%" + c_items[0] + "') AND scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword); 
                            break;
                        case 2:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE " +
                                 "(System.FileName LIKE '%" + c_items[0] +
                                 "' OR System.FileName LIKE '%" + c_items[1] +
                                 "') AND scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword); 
                            break;
                        case 3:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE " +
                                 "(System.FileName LIKE '%" + c_items[0] +
                                 "' OR System.FileName LIKE '%" + c_items[1] +
                                 "' OR System.FileName LIKE '%" + c_items[2] +
                                 "') AND scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword); 
                            break;
                        case 4:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE " +
                                 "(System.FileName LIKE '%" + c_items[0] +
                                 "' OR System.FileName LIKE '%" + c_items[1] +
                                 "' OR System.FileName LIKE '%" + c_items[2] +
                                 "' OR System.FileName LIKE '%" + c_items[3] +
                                 "') AND scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword);
                            break;
                        case 5:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE " +
                                 "(System.FileName LIKE '%" + c_items[0] +
                                 "' OR System.FileName LIKE '%" + c_items[1] +
                                 "' OR System.FileName LIKE '%" + c_items[2] +
                                 "' OR System.FileName LIKE '%" + c_items[3] +
                                 "' OR System.FileName LIKE '%" + c_items[4] +
                                 "') AND scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword); 
                            break;
                        case 6:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE " +
                                 "(System.FileName LIKE '%" + c_items[0] +
                                 "' OR System.FileName LIKE '%" + c_items[1] +
                                 "' OR System.FileName LIKE '%" + c_items[2] +
                                 "' OR System.FileName LIKE '%" + c_items[3] +
                                 "' OR System.FileName LIKE '%" + c_items[4] +
                                 "' OR System.FileName LIKE '%" + c_items[5] +
                                 "') AND scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword);
                            break;
                        case 7:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE " +
                                 "(System.FileName LIKE '%" + c_items[0] +
                                 "' OR System.FileName LIKE '%" + c_items[1] +
                                 "' OR System.FileName LIKE '%" + c_items[2] +
                                 "' OR System.FileName LIKE '%" + c_items[3] +
                                 "' OR System.FileName LIKE '%" + c_items[4] +
                                 "' OR System.FileName LIKE '%" + c_items[5] +
                                 "' OR System.FileName LIKE '%" + c_items[6] +
                                 "') AND scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword); 
                            break;
                        case 8:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE " +
                                 "(System.FileName LIKE '%" + c_items[0] +
                                 "' OR System.FileName LIKE '%" + c_items[1] +
                                 "' OR System.FileName LIKE '%" + c_items[2] +
                                 "' OR System.FileName LIKE '%" + c_items[3] +
                                 "' OR System.FileName LIKE '%" + c_items[4] +
                                 "' OR System.FileName LIKE '%" + c_items[5] +
                                 "' OR System.FileName LIKE '%" + c_items[6] +
                                 "' OR System.FileName LIKE '%" + c_items[7] +
                                 "') AND scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword); 
                            break;
                        case 9:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE " +
                                 "(System.FileName LIKE '%" + c_items[0] +
                                 "' OR System.FileName LIKE '%" + c_items[1] +
                                 "' OR System.FileName LIKE '%" + c_items[2] +
                                 "' OR System.FileName LIKE '%" + c_items[3] +
                                 "' OR System.FileName LIKE '%" + c_items[4] +
                                 "' OR System.FileName LIKE '%" + c_items[5] +
                                 "' OR System.FileName LIKE '%" + c_items[6] +
                                 "' OR System.FileName LIKE '%" + c_items[7] +
                                 "' OR System.FileName LIKE '%" + c_items[8] +
                                 "') AND scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword); 
                            break;

                        case 10:
                            query = @"SELECT System.ItemName, System.ItemPathDisplay,System.ItemType,System.DateCreated,System.DateModified,System.DateAccessed,SYSTEM.SIZE,SYSTEM.ITEMURL FROM SystemIndex " +
                                 "WHERE " +
                                 "(System.FileName LIKE '%" + c_items[0] +
                                 "' OR System.FileName LIKE '%" + c_items[1] +
                                 "' OR System.FileName LIKE '%" + c_items[2] +
                                 "' OR System.FileName LIKE '%" + c_items[3] +
                                 "' OR System.FileName LIKE '%" + c_items[4] +
                                 "' OR System.FileName LIKE '%" + c_items[5] +
                                 "' OR System.FileName LIKE '%" + c_items[6] +
                                 "' OR System.FileName LIKE '%" + c_items[7] +
                                 "' OR System.FileName LIKE '%" + c_items[8] +
                                 "' OR System.FileName LIKE '%" + c_items[9] +
                                 "') AND scope ='file:" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer) + String.Format("' and FREETEXT('{0}') ORDER BY System.DateModified DESC", keyword); 
                            break;

                        default:
                            break;
                    }

                }

                /* Windows Search Database Connection */
                OleDbConnection connection = new OleDbConnection(connectionString);
                OleDbCommand command = new OleDbCommand(query, connection);
                connection.Open();

                /* Database Result Variables*/
                List<string> result_name = new List<string>();
                List<string> result_path = new List<string>();
                List<string> result_ItemType = new List<string>();
                List<string> result_DateCreated = new List<string>();
                List<string> result_DateModified = new List<string>();
                List<string> result_DateAccessed = new List<string>();
                List<double> result_Size = new List<double>();
                List<string> result_url = new List<string>();

                /* Filling the list from database query records */
                if (Keyword_Text.Text != @"Enter text here...")
                {

                    OleDbDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        try { result_name.Add(reader.GetString(0));} catch (Exception ex) { result_name.Add(" "); }
                        try { result_path.Add(reader.GetString(1)); } catch (Exception ex) { result_path.Add(" "); }
                        try { result_ItemType.Add(reader.GetString(2)); } catch (Exception ex) { result_ItemType.Add(" "); }
                        try { result_DateCreated.Add(Convert.ToString(reader[3])); } catch (Exception ex) { result_DateCreated.Add(" "); }
                        try { result_DateModified.Add(Convert.ToString(reader[4])); } catch (Exception ex) { result_DateModified.Add(" "); }
                        try { result_DateAccessed.Add(Convert.ToString(reader[5])); } catch (Exception ex) { result_DateAccessed.Add(" "); }
                        try { result_Size.Add(Convert.ToDouble(reader[6]) / (1024 * 1000)); } catch (Exception ex) { result_Size.Add(0.00); }
                        try { result_url.Add(Convert.ToString(reader[7])); } catch (Exception ex) { result_url.Add(" "); } finally { try { path_url.Add(reader.GetString(1), Convert.ToString(reader[7])); } catch { } }
                    }
                    connection.Close(); //

                    Size maxnamesize = new Size(10, 10); // maxnamesize will be used for manual column size adjustment of datagridview
                    Font Fontsize = dataGridView1.DefaultCellStyle.Font;

                     /* Filling the data rows from list of query records */
                    for (int i = 0; i < result_name.Count; i++)
                    {
                        dt.Rows.Add(result_name[i]);
                        dt.Rows[i]["Name"] = result_name[i].ToString();
                        dt.Rows[i]["Path"] = result_path[i].ToString();
                        dt.Rows[i]["ItemType"] = result_ItemType[i].ToString();
                        dt.Rows[i]["DateCreated"] = result_DateCreated[i].ToString();
                        dt.Rows[i]["DateModified"] = result_DateModified[i].ToString();
                        dt.Rows[i]["DateAccessed"] = result_DateAccessed[i].ToString();
                        dt.Rows[i]["Size (MB)"] = result_Size[i].ToString("N2");
                        dataGridView1.DataSource = dt;

                        /* Extension filter implementation*/
                        if (Extension_Text.Text != ".*" || Extension_Text.Text != "" || !string.IsNullOrEmpty(Extension_Text.Text))
                        {
                            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = string.Format("ItemType like '%{0}%'", Extension_Text.Text.ToLower());
                        }

                        // Get the file name text as Pixel
                        if (TextRenderer.MeasureText(result_name[i].ToString(), Fontsize).Width > (int)maxnamesize.Width)
                        {
                            maxnamesize.Width = TextRenderer.MeasureText(result_name[i].ToString(), Fontsize, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
                        }
                    }
                    dataGridView1.Visible = true;
                    dataGridView1.Refresh();
                    Count_of_display.Visible = true;
                    Count_of_display.Text = String.Format("Count of display is : {0}", result_name.Count);

                    if (maxnamesize.Width < (int)(dataGridView1.Width * 0.38))
                    {
                        if (maxnamesize.Width < 100)
                        {
                            dataGridView1.Columns[0].Width = 100;
                        }
                        else
                        {
                            dataGridView1.Columns[0].Width = maxnamesize.Width;
                        }
                    }
                    else
                    {
                        dataGridView1.Columns[0].Width = (int)(dataGridView1.Width * 0.38);
                    }

                    dataGridView1.Columns[1].Width = (int)(dataGridView1.Width * 0.1);
                    dataGridView1.Columns[2].Width = (int)(dataGridView1.Width * 0.1);
                    dataGridView1.Columns[3].Width = (int)(dataGridView1.Width * 0.1);
                    dataGridView1.Columns[4].Width = (int)(dataGridView1.Width * 0.08);
                    dataGridView1.Columns[5].Width = (int)(dataGridView1.Width * 0.08);
                    dataGridView1.Columns[6].Width = (int)(dataGridView1.Width * 0.08);
                    dataGridView1.Columns[7].Width = (int)(dataGridView1.Width * 0.08);
                    //   dataGridView1.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                else
                {
                    Count_of_display.Visible = false;
                    dataGridView1.Visible = false;
                    dataGridView1.Refresh();

                }
            }

        }


        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            StartProcess(dataGridView1.CurrentRow.Cells[1].Value.ToString(), dataGridView1.CurrentRow.Index); // Open the search result with default program
        }

        private void StartProcess(string path, int _row_index)
        {
            ProcessStartInfo StartInformation = new ProcessStartInfo();
            try
            {
                if (path.Substring(0, 1) != @"/") 
                {
                    StartInformation.FileName = path_url[path];
                    StartInformation.WorkingDirectory = Path.GetDirectoryName(path);
                    using (Process process = new Process())
                    {
                        process.EnableRaisingEvents = true;
                        process.StartInfo = StartInformation;
                        process.Start();
                    }
                }
                else
                {
                    string dic_url = path_url[path];
                    System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(dic_url.Replace("mapi16", "mapi"));//itemUrl is string like "mapi://{S-1-5-21-1637761892-2229936158-1165770529-1000}/Personal Folders($f89c2bd7)/0/Inbox/가가가가겾겉겙공겑걶걩걂겟겛겁곘강곾걣겷겄갚갠가""
                    info.UseShellExecute = true;
                    try
                    {
                        System.Diagnostics.Process res = System.Diagnostics.Process.Start(info);
                    }
                    catch
                    {

                    }
                }
            }

            catch (Exception e)
            {
                MessageBox.Show("Indexing is experiencing issue, please try it later", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            var checkedfile = (DataRowView)dataGridView1.Rows[_row_index].DataBoundItem;
            checkedfile["Checked ?"] = "Controlled";
            checkedfile.EndEdit();

        }

        private async void Keyword_Text_TextChanged(object sender, EventArgs e)
        {
            if (Keyword_Text.Text != "Enter text here..." && Keyword_Text.Text != "")
            {
                       await Task.Delay(100);
                CheckTimer(() => { Search.PerformClick(); });
            }
        }
        private void CheckTimer(Action act)
        {
            if (_typingTimer == null)
            {
                _typingTimer = new System.Windows.Forms.Timer { Interval = 500 };
                _typingTimer.Tick += (sender, args) =>
                {
                    if (!(sender is System.Windows.Forms.Timer timer))
                        return;
                    act?.Invoke();
                    timer.Stop();
                };
            }
            _typingTimer.Stop();
            _typingTimer.Start();
        }

        private async void Extension_Text_TextChanged(object sender, EventArgs e)
        {
            if (Extension_Text.Text != ".*")
            {
                await Task.Delay(100);
                CheckTimer(() => { Search.PerformClick(); });
            }
        }

        private void FileType_checkedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FileType_checkedListBox.CheckedItems.Count != 0)
            {
                Extension_Text.Enabled = false;
            }
            else
            {
                Extension_Text.Enabled = true;
            }
            Search.PerformClick();
        }

        private void Keyword_Text_MouseLeave(object sender, EventArgs e)
        {
            if (Keyword_Text.Text != "Enter text here...")
            {
                Keyword_Text.ForeColor = Color.Gray;
                AddDefaultText(sender, e);
            }
        }

        private void Extension_Text_MouseLeave(object sender, EventArgs e)
        {
            if (Extension_Text.Text != ".*")
            {
                Extension_Text.ForeColor = Color.Gray;
                AddDefaultText2(sender, e);
            }
        }

        private void Keyword_Text_MouseClick(object sender, MouseEventArgs e)
        {
            if (Keyword_Text.Text == "Enter text here...")
            {
                Keyword_Text.ForeColor = Color.Black;
                RemoveText(sender, e);
            }
        }

        private void Extension_Text_MouseClick(object sender, MouseEventArgs e)
        {
            if (Extension_Text.Text == ".*")
            {
                Extension_Text.ForeColor = Color.Black;
                RemoveText2(sender, e);
            }
        }

        private void Keyword_Text_MouseHover(object sender, EventArgs e)
        {
            if (Keyword_Text.Text == "Enter text here...")
            {
                Keyword_Text.ForeColor = Color.Black;
                RemoveText(sender, e);
            }
        }

        private void Extension_Text_MouseHover(object sender, EventArgs e)
        {
            if (Extension_Text.Text == ".*")
            {
                Extension_Text.ForeColor = Color.Black;
                RemoveText2(sender, e);
            }
        }

        private void dataGridView1_VisibleChanged(object sender, EventArgs e)
        {
            var grid = sender as DataGridView;
            int _count_dg_row = grid.Rows.Count;
            int _color_row = 1;
            for (int i = 0; i <= _count_dg_row - 1; i++)
            {
                if (_color_row == 1)
                {
                    grid.Rows[i].DefaultCellStyle.BackColor = Color.WhiteSmoke;
                    _color_row = 2;
                }
                else
                {
                    grid.Rows[i].DefaultCellStyle.BackColor = Color.AliceBlue;
                    _color_row = 1;
                }
            }
        }


        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGridView1_VisibleChanged(sender, e);
        }

        private void FullTS_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (FullTS_checkBox.Checked == true)
            {
                FileType_checkedListBox.Enabled = false;

            }
            else
            {
                FileType_checkedListBox.Enabled = true;
            }

            Search.PerformClick();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            IDictionary<string, string> map = new Dictionary<string, string>()
            {
                {@"file:",""},
                {@"/",@"\"}
            };
            var regex = new Regex(String.Join("|", map.Keys));

            try
            {
                if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control && mouse_over_datagrid == 1)
                {
                    Thread.Sleep(200);
                    StringCollection paths = new StringCollection();
                    foreach (DataGridViewRow r in dataGridView1.SelectedRows)
                    {
                        string _path = r.Cells[1].Value.ToString();
                        try
                        {
                            if (_path.Substring(0, 1) != @"/" || !string.IsNullOrEmpty(_path) || _path != "")
                            {
                                paths.Add(regex.Replace(path_url[r.Cells[1].Value.ToString()], m => map[m.Value]));
                            }
                            else
                            {
                                //      MessageBox.Show("outlook plugin service is not available on demo version", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        Thread t = new Thread(() =>
                        {
                            Clipboard.SetFileDropList(paths);

                        });
                        t.SetApartmentState(ApartmentState.STA);
                        t.Start();
                    } catch { }

                    if (dataGridView1.SelectedRows.Count > 0 && paths.Count > 0) { }
                    else if (dataGridView1.SelectedRows.Count > 0 && paths.Count == 0)
                    {
                        MessageBox.Show("The Indexed file does not exist or outlook plugin service is not available", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }

                if (e.KeyCode == Keys.Enter && dataGridView1.SelectedRows.Count > 0)
                {
                    e.SuppressKeyPress = true;   // avoid to extra move next row on datagrid
                    foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                    {
                        dataGridView1_CellDoubleClick(sender, new DataGridViewCellEventArgs(0, row.Index));
                    }
                }
            } catch { }
        }


        private void dataGridView1_MouseEnter(object sender, EventArgs e)
        {
            mouse_over_datagrid = 1;
        }

        private void dataGridView1_MouseLeave(object sender, EventArgs e)
        {
            mouse_over_datagrid = 2;
        }

        private void dataGridView1_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
        {
            int _count_dg_row = dt.Rows.Count;
            for (int i = 0; i <= _count_dg_row - 1; i++)
            {
                if (dt.Rows[i]["Checked ?"].ToString() == "Controlled")
                {
                    DataGridViewRow theRow = dataGridView1.Rows[i];
                    theRow.DefaultCellStyle.ForeColor = Color.LimeGreen;
                }
            }
        }

        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            Thread.Sleep(200);
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[7].Value.ToString().Equals("Controlled"))
                {
                    DataGridViewRow theRow = dataGridView1.Rows[row.Index];
                    theRow.DefaultCellStyle.ForeColor = Color.LimeGreen;
                }
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            this.Visible = true;
        }


        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void OnlyEmails_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (OnlyEmails_checkBox.Checked == true)
            {
                FullTS_checkBox.Checked = false;
                FullTS_checkBox.Enabled = false;
                FileType_checkedListBox.Enabled = false;
            }
            else
            {
                FullTS_checkBox.Enabled = true;
                FileType_checkedListBox.Enabled = true;
            }
            Search.PerformClick();
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about__form = new AboutBox1();
            about__form.ShowDialog();
        }

    }

}
