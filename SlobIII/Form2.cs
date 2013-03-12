using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;

namespace SlobIII
{
    public partial class Form2 : Form
    {
        private DB db = null;
        public Form2(DB db)
        {            
            InitializeComponent();
            this.db = db;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            TreeNode root = new TreeNode(db.getDbName()); 
            DataSet ds = db.Select("SELECT tablename FROM pg_tables WHERE schemaname = 'public' ");
            
             
            this.treeView1.Nodes.Add(root);
            for (int i = 0; i <  ds.Tables[0].Rows.Count; i++) {
                root.Nodes.Add(ds.Tables[0].Rows[i][0].ToString(),ds.Tables[0].Rows[i][0].ToString(),1,2);
            }
            root.Toggle();

            setMsg( root.Text + " 加载成功！数据表共计" + ds.Tables[0].Rows.Count + "张");

            listView1.Columns.Add("字段", 200);
            listView1.Columns.Add("类型", 300); 
            listView1.CheckBoxes = true;
            listView1.View = View.Details;
             
        }
        Dictionary<string, DataSet> hash = new Dictionary<string, DataSet>();
        Dictionary<string, string> colType = new Dictionary<string, string>();
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            colType = new Dictionary<string, string>();
            listView1.Items.Clear();
            TreeView tv = (TreeView)sender;
            if (tv.SelectedNode.Nodes.Count == 0)
            { 
                string tableName = tv.SelectedNode.Name;
                DataSet ds = null;
                if (hash.ContainsKey(tableName))
                {
                    ds = hash[tableName];
                }
                else
                {
                    string sql = "SELECT  a.attname AS name ,format_type(a.atttypid, a.atttypmod) AS type FROM  pg_class AS c,pg_attribute AS a WHERE a.attrelid = c.oid  AND a.attnum > 0   AND c.relkind='r'  AND c.relname = '" + tableName + "'";
                    ds = db.Select(sql);
                    hash.Add(tableName, ds);
                }
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    listView1.Items.Add(new ListViewItem(new string[] { ds.Tables[0].Rows[i][0].ToString(), ds.Tables[0].Rows[i][1].ToString().ToUpper() }));
                    listView1.Items[i].Checked = true;
                    colType.Add(tableName + "_" + ds.Tables[0].Rows[i][0].ToString(), ds.Tables[0].Rows[i][1].ToString());
                } 
                
                setMsg(" 表：" + tableName);
            }
        }
        void setMsg(string msg) {
            this.label1.Text = msg;
        }
        void wfile(string tabname, string cols, string insql, FileStream fs, DataTable table)
        {  
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string colstr = "";
                for (int j = 0; j < table.Columns.Count; j++) 
                {
                     
                        string colName = table.Columns[j] + "";
                        object obj = table.Rows[i][colName];
                        if (isNull(obj))
                        {
                            colstr += "null";
                        } 
                        else if (isInt(obj))
                        {
                            colstr += obj;
                        }
                        else
                        {
                            colstr += "'" + obj.ToString().Replace("'", "''") + "'";
                        }
                        colstr += ","; 
                }
                colstr = colstr.Substring(0, colstr.Length - 1);
                string s = insql + "(" + colstr + "); \n";

                byte[] data = new UTF8Encoding().GetBytes(s);
                progressBar1.Value = progressBar1.Value+1;
                setMsg(" 已经生成：" + progressBar1.Value + "条数据");
                fs.Write(data, 0, data.Length);
                
            }
        }
        Boolean isNull(Object obj) {
            if (obj.Equals(null) || (obj.GetType().ToString().Equals("System.DBNull")))
            {
                return true;
            }
            return false;
        }
        Boolean isInt(Object obj)
        { 
            if(obj.GetType().ToString().ToLower().IndexOf("int")>0){
                return true;
            }
            return false;
        }
        private void button1_Click(object sender, EventArgs e)
        { 
            //导出文件 
            if (this.tabControl1.SelectedIndex == 0)
            {
                //导出表
                if (this.treeView1.SelectedNode.Nodes.Count != 0)
                {
                    MessageBox.Show("请选择数据表！");
                    return;
                }
                string tabname = this.treeView1.SelectedNode.Name;
                string cols = "";
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[i].Checked)
                    {
                        cols += listView1.Items[i].Text + ",";
                    }
                }
                if (cols == "") { MessageBox.Show("请勾选需要查询的字段"); return; }
                cols = cols.Substring(0, cols.Length - 1);

                string countSQL = "SELECT COUNT(1) FROM " + tabname;
                DataTable countTable = (db.Select(countSQL)).Tables[0];
                int count = int.Parse(countTable.Rows[0][0].ToString());
                progressBar1.Maximum = count;
                progressBar1.Value = 0;

                SaveFileDialog fileDialog1 = new SaveFileDialog();
                fileDialog1.InitialDirectory = "d://";
                fileDialog1.FileName = "[" + DateTime.Now.ToString("yyyyMMdd") + "]INSERT-" + tabname + ".sql";
                fileDialog1.Filter = "sql (*.sql)|*.sql|All files (*.*)|*.*";
                fileDialog1.FilterIndex = 1;
                fileDialog1.RestoreDirectory = true;

                if (fileDialog1.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(fileDialog1.FileName, FileMode.Create,FileAccess.Write );
                    
                    string insql = " INSERT INTO " + tabname + " (" + cols + ") VALUES";

                    int offset = 0;
                    int sumCount = 0;
                    while (offset < count)
                    { 
                        string sql = "SELECT " + cols + " FROM " + tabname + " LIMIT 1000 OFFSET " + offset; 
                        DataTable table = (db.Select(sql)).Tables[0];
                        sumCount += table.Rows.Count; 
                        wfile(tabname, cols, insql, fs, table);
                        if ((offset + 1000) > count) { offset = count; }
                        else
                        {
                            offset += 1000;
                        }
                    }

                    //清空缓冲区、关闭流
                    fs.Flush();
                    fs.Close();

                    MessageBox.Show("完成！");
                }
            }
            else {
                //自定义SQL查询
                string sql = this.textBox1.Text;
                DataTable table;
                try {
                     table = (db.Select(sql)).Tables[0];
                }catch(Exception ex){
                    MessageBox.Show("查询错误："+ex.Message);
                    return;
                }
                progressBar1.Maximum = table.Rows.Count;
                progressBar1.Value = 0;
                string cols = "";
                
                 
                for(int i=0;i<table.Columns.Count;i++)
                { 
                    cols += table.Columns[i]  + ","; 
                }
                if (cols == "") { MessageBox.Show("没有查询到任何列"); return; }
                cols = cols.Substring(0, cols.Length - 1);

                string tabname = this.textBox2.Text ;

                 SaveFileDialog fileDialog1 = new SaveFileDialog();
                fileDialog1.InitialDirectory = "d://";
                fileDialog1.FileName = "[" + DateTime.Now.ToString("yyyyMMdd") + "]INSERT-" + tabname + ".sql";
                fileDialog1.Filter = "sql (*.sql)|*.sql|All files (*.*)|*.*";
                fileDialog1.FilterIndex = 1;
                fileDialog1.RestoreDirectory = true;

                if (fileDialog1.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(fileDialog1.FileName, FileMode.Create, FileAccess.Write);
                    string insql = " INSERT INTO " + tabname + " (" + cols + ") VALUES";

                    wfile(tabname, cols, insql, fs, table);
                    //清空缓冲区、关闭流
                    fs.Flush();
                    fs.Close();

                    MessageBox.Show("完成！");
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        
    }
}