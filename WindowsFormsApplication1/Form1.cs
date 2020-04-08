using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        DataTable mytable = new DataTable();
        SqlConnection myconn = new SqlConnection();
        SqlTransaction mytxn;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //  Establish a connection
        
            myconn.ConnectionString = " Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\pvfc\\PVFC.mdf;Integrated Security=True;Connect Timeout=30";
            myconn.Open();
           

            //  Make a SQLCommand object

            SqlCommand mycommand = new SqlCommand();
          //  mycommand.CommandText = " Select * from Customer_T where CustomerState = '" + textBox1.Text + "'";
            

            mycommand.CommandText = " Select * from Customer_T with (READUncommitted)  ";


            mycommand.Parameters.Add("@state", SqlDbType.NChar,20);
            mycommand.Parameters["@state"].Value = textBox1.Text;
            mycommand.Parameters.Add("@name", SqlDbType.NVarChar, 50);
            mycommand.Parameters["@name"].Value = "%" + textBox2.Text + "%";  
            mycommand.Connection = myconn;

            //   Create an Adapter  (messenger carrying our request)

            SqlDataAdapter myadapter = new SqlDataAdapter();
            myadapter.SelectCommand = mycommand;

            //   Fill an internal table
           
            myadapter.Fill(mytable);

            //   Bind the table to the GUI object
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = mytable;
            


            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(mytable.Rows[1].ItemArray[2].ToString());
        }

        private void Form1_Click(object sender, EventArgs e)
        {
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            mytable.Rows[1].SetField(2,"ABC");
        }

        private void btnUpdateDB_Click(object sender, EventArgs e)
        {

            //    updcmd.CommandText = "Update Customer_T set CustomerName = @customername," + "CustomerAddress = @customeraddress, CustomerCity = @customercity,"
            //+ " CustomerState = @customerstate, CustomerPostalCode = @customerpostalcode" + " where CustomerID = @customerid";


            mytxn = myconn.BeginTransaction(IsolationLevel.ReadUncommitted);
            SqlCommand updcmd;   
            updcmd = new SqlCommand();
            updcmd.Transaction = mytxn;
            updcmd.Connection = myconn;
            updcmd.CommandText = "Update Customer_T set CustomerName = @customername " + "where CustomerID = @customerid and" +
                " CustomerVersion = @version";
            updcmd.Parameters.Add("@version", SqlDbType.Binary, 50, "CustomerVersion");
            updcmd.Parameters.Add("@customername", SqlDbType.NVarChar, 50,"CustomerName");
            updcmd.Parameters.Add("@customerid", SqlDbType.Int, 50, "CustomerID");
            SqlDataAdapter myadapter = new SqlDataAdapter();
            myadapter.UpdateCommand = updcmd;
          
            SqlCommand delcmd;
            delcmd = new SqlCommand();
            delcmd.Connection = myconn;
            delcmd.CommandText = "Delete Customer_T where CustomerID = @customerid ";
            delcmd.Parameters.Add("@customerid", SqlDbType.Int, 50, "CustomerID");
            
            myadapter.DeleteCommand = delcmd;
            myadapter.ContinueUpdateOnError = true;

            try
            {
                myadapter.Update(mytable);
                

                //mytxn.Commit();
            }
            catch (Exception ex)
            {
                mytxn.Rollback();
                MessageBox.Show("Database has been updated - Please refill grid and make updates");
                // do the Fill (from above) again
            }



        }

        private void button4_Click(object sender, EventArgs e)
        {
            mytxn.Commit();
        }
    }
}
