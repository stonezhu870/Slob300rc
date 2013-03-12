using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Npgsql;

namespace SlobIII
{
    public class DB
    {
        private string server = "";
        private string port = "";
        private string userid = "";
        private string pwd = "";
        private string dbname = "";
        private string ConnectionString = "";
        private NpgsqlConnection connect = null;
        public string getDbName() {
            return dbname;
        }
        public DB( string server,string port,string userid,string pwd ,string dbname) {
            this.server = server;
            this.port = port;
            this.userid = userid;
            this.pwd = pwd;
            this.dbname = dbname;
            ConnectionString = "Server=" + server + ";Port=" + port + ";User Id=" + userid + ";Password=" + pwd + ";Database=" + dbname + "";
            connect = new NpgsqlConnection(ConnectionString);
            //Server=192.168.0.166;Port=5432;User Id=cms;Password=cms;Database=zj_cms
        }

       
        //以上是加载驱动你要连到的数据库  
        public string test(){
            try {
                connect.Open();
            }catch(Exception e){
                return e.Message;
            }
            return null;
 
        }
 

        public DataSet Select(string sql)
        {
            DataSet objDataSet = new DataSet();

            NpgsqlDataAdapter objSqlDataAdapter = new NpgsqlDataAdapter(sql, connect); 
            objSqlDataAdapter.Fill(objDataSet);
            return objDataSet;
        }
        //这个方法是查； 
 
    } 
}
