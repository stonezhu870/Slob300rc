using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace SlobIII
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkDatabase()!=null)
            {
                MessageBox.Show("数据库连接成功！");
            }

        }
        private DB checkDatabase()
        {
            string server = txt_server.Text.Trim();
            string port = txt_port.Text.Trim();
            string userid = txt_userid.Text.Trim();
            string pwd = txt_pwd.Text.Trim();
            string dbname = txt_dbname.Text.Trim();
            DB db = new DB(server, port, userid, pwd, dbname);
            string msg = db.test();
            if (null == msg)
            {
                return db;
            }
            else
            {
                MessageBox.Show("数据库连接失败：" + msg);
                return null;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DB db = checkDatabase();
            if (db!=null)
            {
                Form2 form2 = new Form2(db);
                form2.ShowDialog();
            }
            
            
        }
    }
}