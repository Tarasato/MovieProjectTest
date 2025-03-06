using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieProjectTest
{
    internal class ShareData
    {
        public static void showWarningMSG(string msg)
        {
            MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        //connection string
        public static string serverName = "Tarasato-PC\\SQLEXPRESS";
        public static string conStr = "Server="+ serverName + ";Database=movie_record_db;Trusted_connection=True";

    }
}
