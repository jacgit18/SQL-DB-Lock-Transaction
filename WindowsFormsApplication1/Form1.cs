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
{  // Make a desktop application that maintains the data in the Employee Table.
    // Make a FILL button to search on a value contained in the Employee Name.
    // Your application should be able to add, delete and update, and check 
    // for concurrency.Also, see what happens if you put a Supervisor ID 
    // that is not in the table(foreign-key reference error).
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


            // going to database for transaction of data
            // allow user to read data while updating this by default isnt enabled
            // this is known as isolation level
            mytxn = myconn.BeginTransaction(IsolationLevel.ReadUncommitted);
            SqlCommand updcmd;   
            updcmd = new SqlCommand();
            updcmd.Transaction = mytxn;
            updcmd.Connection = myconn;
            //updcmd.CommandText = "Update Customer_T set CustomerName = @customername " + "where CustomerID = @customerid and" +
            //    " CustomerVersion = @version";

            // alternative to above
            updcmd.CommandText = "GetAllCustomer";
            updcmd.CommandType = CommandType.StoredProcedure;


            updcmd.Parameters.Add("@version", SqlDbType.Binary, 50, "CustomerVersion");

            // locking record temporary for changes but locks can be buggy


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

            // changinging data is important take into account if data is link like if you change data of someone in a
            // insurance database
            // you wouldnt want update to continue if there are multiple updates being done at same time then system try to update 
            // the same time you would want it to stop and not continue or but if rows and changes are indepent you would
            // want updates to continue since they arent related thus you wont get alot of problems 
            try
            {
                myadapter.Update(mytable);
                // usually done here
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
            // doing here for testing purposes

            mytxn.Commit();
        }
    }
}
