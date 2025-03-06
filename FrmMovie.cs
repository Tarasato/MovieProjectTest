using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieProjectTest
{
    public partial class FrmMovie : Form
    {

        byte[] movieImg, movieDirImg;

        public FrmMovie()
        {
            InitializeComponent();
        }

        public void toggleButton(bool tf) {
            tbMovieName.Enabled = tf;
            tbMovieDetail.Enabled = tf;
            dtpMovieDateSale.Enabled = tf;
            nudMovieHour.Enabled = tf;
            nudMovieMinute.Enabled = tf;
            cbbMovieType.Enabled = tf;
            tbMovieDVDTotal.Enabled = tf;
            tbMovieDVDPrice.Enabled = tf;
            btSaveAddEdit.Enabled = tf;
            btEdit.Enabled = tf;
            btDel.Enabled = tf;
            btSelectImg1.Enabled = tf;
            btSelectImg2.Enabled = tf;
            btAdd.Enabled = !tf;
        }

        public void clearUI()
        {
            lbMovieId.Text = "";
            tbMovieName.Text = "";
            tbMovieDetail.Text = "";
            dtpMovieDateSale.Value = DateTime.Now;
            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;
            cbbMovieType.SelectedIndex = 0;
            tbMovieDVDTotal.Text = "0";
            tbMovieDVDPrice.Text = "0.00";
            pcbMovieImg.Image = null;
            pcbDirMovie.Image = null;
        }

        private void LoadMovieDetails(string movieId)
        {
            using (SqlConnection conn = new SqlConnection(ShareData.conStr))
            {
                try
                {
                    conn.Open();
                    string strSql = @"
                SELECT 
                    *
                FROM movie_tb m
                INNER JOIN movie_type_tb t ON m.movieTypeId = t.movieTypeId
                WHERE m.movieId = @movieId";

                    using (SqlCommand cmd = new SqlCommand(strSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@movieId", movieId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                //แสดงข้อมูลใน TextBox และ Label
                                tbMovieName.Text = reader["movieName"].ToString(); ;
                                tbMovieDetail.Text = reader["movieDetail"].ToString();
                                dtpMovieDateSale.Value = Convert.ToDateTime(reader["movieDateSale"].ToString());
                                nudMovieHour.Value = (int)reader["movieLengthHour"];
                                nudMovieMinute.Value = (int)reader["movieLengthMinute"];
                                cbbMovieType.SelectedIndex = (int)reader["movieTypeId"] - 1;
                                tbMovieDVDTotal.Text = reader["movieDVDTotal"].ToString();
                                tbMovieDVDPrice.Text = reader["movieDVDPrice"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        public void LoadData()
        {
            dgvMovieShowAll.Columns.Clear(); // ล้าง Column ใน DataGridView
            lsMovieShow.Items.Clear(); // ล้าง ListView

            toggleButton(false);
            clearUI();

            // Connect to DB
            SqlConnection conn = new SqlConnection(ShareData.conStr);
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn.Open();

            // SQL Command
            string strSql = @"
SELECT 
    m.movieId, 
    m.movieName, 
    m.movieDetail, 
    m.movieDateSale, 
    t.movieTypeName 
FROM movie_tb m
INNER JOIN movie_type_tb t ON m.movieTypeId = t.movieTypeId";

            // Create SQL command and SQL transaction for working with SQL
            SqlTransaction sqlTransaction = conn.BeginTransaction();
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = conn;
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.CommandText = strSql;
            sqlCommand.Transaction = sqlTransaction;

            // Run SQL
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            conn.Close();

            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // Get data in DataTable and display in DGV
            if (dt.Rows.Count > 0)
            {
                // สร้าง CultureInfo สำหรับภาษาไทย
                CultureInfo thaiCulture = new CultureInfo("th-TH");

                // กำหนดคอลัมน์ใน DataGridView ก่อนเพิ่มข้อมูล
                if (dgvMovieShowAll.Columns.Count == 0)
                {
                    dgvMovieShowAll.Columns.Add("movieId", "รหัสภาพยนต์");
                    dgvMovieShowAll.Columns.Add("movieName", "ชื่อภาพยนต์");
                    dgvMovieShowAll.Columns.Add("movieDetail", "รายละเอียด");
                    dgvMovieShowAll.Columns.Add("movieDateSale", "วันที่วางขาย");
                    dgvMovieShowAll.Columns.Add("movieTypeName", "หมวดหมู่");
                }

                // เคลียร์ข้อมูลใน DataGridView ก่อน
                dgvMovieShowAll.Rows.Clear();

                // สร้าง DataGridView แบบแสดงข้อมูลทีละ Row
                foreach (DataRow row in dt.Rows)
                {
                    // แปลงวันที่เป็นภาษาไทย
                    DateTime movieDateSale = Convert.ToDateTime(row["movieDateSale"]);
                    string movieDateSaleThai = movieDateSale.ToString("dd MMM yyyy", thaiCulture);

                    // สร้าง DataGridViewRow ใหม่
                    DataGridViewRow dgvRow = new DataGridViewRow();

                    // สร้างและเพิ่ม cell สำหรับ movieId
                    dgvRow.Cells.Add(new DataGridViewTextBoxCell { Value = row["movieId"].ToString() });

                    // สร้างและเพิ่ม cell สำหรับ movieName
                    dgvRow.Cells.Add(new DataGridViewTextBoxCell { Value = row["movieName"].ToString() });

                    // สร้างและเพิ่ม cell สำหรับ movieDetail
                    dgvRow.Cells.Add(new DataGridViewTextBoxCell { Value = row["movieDetail"].ToString() });

                    // สร้างและเพิ่ม cell สำหรับ movieDateSale (วันที่แปลงแล้วเป็นภาษาไทย)
                    dgvRow.Cells.Add(new DataGridViewTextBoxCell { Value = movieDateSaleThai });

                    // สร้างและเพิ่ม cell สำหรับ movieTypeName
                    dgvRow.Cells.Add(new DataGridViewTextBoxCell { Value = row["movieTypeName"].ToString() });

                    // เพิ่ม Row ไปที่ DataGridView
                    dgvMovieShowAll.Rows.Add(dgvRow);
                }

                dgvMovieShowAll.EnableHeadersVisualStyles = false;

                // กำหนดขนาด column DGV
                dgvMovieShowAll.Columns[0].Width = 100;
                dgvMovieShowAll.Columns[1].Width = 150;
                dgvMovieShowAll.Columns[2].Width = 150;
                dgvMovieShowAll.Columns[3].Width = 100;
                dgvMovieShowAll.Columns[4].Width = 80;

                // เพิ่มข้อมูลลงใน ListView
                foreach (DataRow row in dt.Rows)
                {
                    ListViewItem item = new ListViewItem(row["movieId"].ToString());
                    item.SubItems.Add(row["movieName"].ToString());

                    // แปลงวันที่เป็นภาษาไทย
                    DateTime movieDateSale = Convert.ToDateTime(row["movieDateSale"]);
                    string movieDateSaleThai = movieDateSale.ToString("dd MMM yyyy", thaiCulture);
                    item.SubItems.Add(movieDateSaleThai); // แสดงวันที่เป็นภาษาไทยใน ListView

                    // เพิ่ม item เข้าไปใน ListView
                    lsMovieShow.Items.Add(item);

                }
            }

            dgvMovieShowAll.ClearSelection(); // ยกเลิกการเลือกใน DataGridView
        }

        private void FrmMovie_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void lsMovieShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsMovieShow.SelectedItems.Count > 0)
            {
                btAdd.Enabled = false;
                btEdit.Enabled = true;
                btDel.Enabled = false;

                // ดึง item ที่เลือก
                ListViewItem selectedItem = lsMovieShow.SelectedItems[0];

                // ดึง movieId และ movieName จาก ListView
                string movieId = selectedItem.SubItems[0].Text;
                string movieName = selectedItem.SubItems[1].Text;

                // แสดง movieId และ movieName ใน Label และ TextBox
                lbMovieId.Text = movieId;
                tbMovieName.Text = movieName;

                // เชื่อมต่อฐานข้อมูลและดึงข้อมูลภาพยนตร์
                using (SqlConnection conn = new SqlConnection(ShareData.conStr))
                {
                    try
                    {
                        conn.Open();

                        string strSql = "SELECT movieId, movieName, movieDetail, movieDateSale, movieLengthHour, movieLengthMinute, movieTypeId, movieDVDTotal, movieDVDPrice, movieImg, movieDirImg " +
                                        "FROM movie_tb WHERE movieId = @movieId";

                        using (SqlCommand cmd = new SqlCommand(strSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@movieId", movieId);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // แสดงข้อมูลในฟอร์ม
                                    tbMovieName.Text = reader["movieName"].ToString();
                                    tbMovieDetail.Text = reader["movieDetail"].ToString();
                                    dtpMovieDateSale.Value = Convert.ToDateTime(reader["movieDateSale"]);
                                    nudMovieHour.Value = Convert.ToInt32(reader["movieLengthHour"]);
                                    nudMovieMinute.Value = Convert.ToInt32(reader["movieLengthMinute"]);
                                    cbbMovieType.SelectedIndex = Convert.ToInt32(reader["movieTypeId"]) - 1;
                                    tbMovieDVDTotal.Text = reader["movieDVDTotal"].ToString();
                                    tbMovieDVDPrice.Text = reader["movieDVDPrice"].ToString();

                                    // ดึงรูปภาพจากฐานข้อมูลและแสดงใน PictureBox
                                    byte[] imageBytes = reader["movieImg"] as byte[];
                                    if (imageBytes != null)
                                    {
                                        using (MemoryStream ms = new MemoryStream(imageBytes))
                                        {
                                            pcbMovieImg.Image = Image.FromStream(ms);  // ใส่ PictureBox ที่คุณใช้แสดงรูปภาพ
                                        }
                                    }

                                    imageBytes = reader["movieDirImg"] as byte[];
                                    if (imageBytes != null)
                                    {
                                        using (MemoryStream ms = new MemoryStream(imageBytes))
                                        {
                                            pcbDirMovie.Image = Image.FromStream(ms);  // ใส่ PictureBox ที่คุณใช้แสดงรูปภาพ
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }

            }
        }

        private void btSelectImg1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //เอารูปที่เลือกมาแสดงใน PictureBox
                pcbMovieImg.Image = Image.FromFile(ofd.FileName);

                //แปลงรูปที่เลือกมาเป็น byte[] เก็บใน travellerImage
                //สร้างตัวแปรเก็บประเภทไฟล์
                string extFile = Path.GetExtension(ofd.FileName);
                //แปลงรูปเป็น byte[]
                using (MemoryStream ms = new MemoryStream())
                {
                    if (extFile == ".jpg" || extFile == ".jpeg")
                    {
                        pcbMovieImg.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    else
                    {
                        pcbMovieImg.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    movieImg = ms.ToArray();
                }
            }
        }

        private void btSelectImg2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //เอารูปที่เลือกมาแสดงใน PictureBox
                pcbDirMovie.Image = Image.FromFile(ofd.FileName);

                //แปลงรูปที่เลือกมาเป็น byte[] เก็บใน travellerImage
                //สร้างตัวแปรเก็บประเภทไฟล์
                string extFile = Path.GetExtension(ofd.FileName);
                //แปลงรูปเป็น byte[]
                using (MemoryStream ms = new MemoryStream())
                {
                    if (extFile == ".jpg" || extFile == ".jpeg")
                    {
                        pcbDirMovie.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    else
                    {
                        pcbDirMovie.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    movieDirImg = ms.ToArray();
                }
            }
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            tbMovieName.Enabled = true;
            tbMovieDetail.Enabled = true;
            dtpMovieDateSale.Enabled = true;
            nudMovieHour.Enabled = true;
            nudMovieMinute.Enabled = true;
            cbbMovieType.Enabled = true;
            tbMovieDVDTotal.Enabled = true;
            tbMovieDVDPrice.Enabled = true;
            btSaveAddEdit.Enabled = true;
            btCancel.Enabled = true;
            btSelectImg1.Enabled = true;
            btSelectImg2.Enabled = true;
            btAdd.Enabled = false;


            // หา movieId ล่าสุด
            string latestMovieId = ""; // กำหนดค่าเริ่มต้น

            using (SqlConnection conn = new SqlConnection(ShareData.conStr))
            {
                try
                {
                    conn.Open();
                    string strSql = "SELECT TOP 1 movieId FROM movie_tb ORDER BY movieId DESC"; // เรียงจากมากไปน้อย
                    using (SqlCommand cmd = new SqlCommand(strSql, conn))
                    {
                        var result = cmd.ExecuteScalar(); // ดึง movieId ล่าสุด

                        if (result != null)
                        {
                            // ถ้ามีข้อมูลในตาราง → ตัด "mv" ออก และแปลงเป็น int
                            string movieIdString = result.ToString().Substring(2); // ตัด "mv"
                            int latestMovieIdInt = Convert.ToInt32(movieIdString); // แปลงเป็น int

                            // เพิ่ม 1 และแปลงกลับเป็น string (ให้เป็นเลข 3 หลัก)
                            latestMovieId = "mv" + (latestMovieIdInt + 1).ToString("D3");
                        }
                        else
                        {
                            // ถ้ายังไม่มีข้อมูลเลย → ให้เริ่มต้นที่ "mv001"
                            latestMovieId = "mv001";
                        }

                        lbMovieId.Text = latestMovieId; // แสดงผล movieId ล่าสุด
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }


            //lbMovieId.Text = "mv" + latestMovieId; // แสดงผล movieId ล่าสุด

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            toggleButton(false);
            clearUI();

            lsMovieShow.SelectedItems.Clear();
        }

        private void btSaveAddEdit_Click(object sender, EventArgs e)
        {
            // Validate ข้อมูล
            if (string.IsNullOrEmpty(tbMovieName.Text))
            {
                ShareData.showWarningMSG("กรุณากรอกชื่อภาพยนต์");
                return;
            }
            else if (string.IsNullOrEmpty(tbMovieDetail.Text))
            {
                ShareData.showWarningMSG("กรุณากรอกรายละเอียดภาพยนต์");
                return;
            }
            else if (nudMovieHour.Value <= 0 && nudMovieMinute.Value <= 0)
            {
                ShareData.showWarningMSG("กรุณากรอกเวลาภาพยนต์");
                return;
            }
            else if (cbbMovieType.SelectedIndex < 0)
            {
                ShareData.showWarningMSG("กรุณาเลือกหมวดหมู่ภาพยนต์");
                return;
            }
            else if (string.IsNullOrEmpty(tbMovieDVDTotal.Text) || int.Parse(tbMovieDVDTotal.Text.Trim()) == 0)
            {
                ShareData.showWarningMSG("กรุณากรอกจำนวน DVD ทั้งหมด");
                return;
            }
            else if (string.IsNullOrEmpty(tbMovieDVDPrice.Text) || decimal.Parse(tbMovieDVDPrice.Text.Trim()) == 0)
            {
                ShareData.showWarningMSG("กรุณากรอกราคา DVD");
                return;
            }
            else if (movieImg == null)
            {
                ShareData.showWarningMSG("กรุณาเลือกรูปภาพภาพยนต์");
            }
            else if (movieDirImg == null)
            {
                ShareData.showWarningMSG("กรุณาเลือกรูปภาพผู้กำกับ");
            }
            else
            {
                // เชื่อมต่อ DB
                using (SqlConnection conn = new SqlConnection(ShareData.conStr))
                {
                    try
                    {
                        conn.Open();

                        // เช็คว่า movieId มีอยู่ในฐานข้อมูลหรือยัง
                        string checkSql = "SELECT COUNT(*) FROM movie_tb WHERE movieId = @movieId";
                        SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                        checkCmd.Parameters.AddWithValue("@movieId", lbMovieId.Text);

                        int count = (int)checkCmd.ExecuteScalar(); // ใช้ ExecuteScalar เพื่อดึงค่าผลลัพธ์เดียว (จำนวนแถวที่มี movieId)

                        string strSql;
                        if (count > 0) // ถ้ามีข้อมูล movieId ในฐานข้อมูล
                        {
                            // อัปเดตข้อมูลที่มีอยู่
                            strSql = @"
                UPDATE movie_tb 
                SET movieName = @movieName, 
                    movieDetail = @movieDetail, 
                    movieDateSale = @movieDateSale, 
                    movieLengthHour = @movieLengthHour, 
                    movieLengthMinute = @movieLengthMinute, 
                    movieTypeId = @movieTypeId, 
                    movieDVDTotal = @movieDVDTotal, 
                    movieDVDPrice = @movieDVDPrice, 
                    movieImg = @movieImg, 
                    movieDirImg = @movieDirImg
                WHERE movieId = @movieId";
                        }
                        else // ถ้าไม่มีข้อมูล movieId ในฐานข้อมูล
                        {
                            // เพิ่มข้อมูลใหม่
                            strSql = @"
                INSERT INTO movie_tb (movieId, movieName, movieDetail, movieDateSale, movieLengthHour, movieLengthMinute, movieTypeId, movieDVDTotal, movieDVDPrice, movieImg, movieDirImg)
                VALUES (@movieId, @movieName, @movieDetail, @movieDateSale, @movieLengthHour, @movieLengthMinute, @movieTypeId, @movieDVDTotal, @movieDVDPrice, @movieImg, @movieDirImg)";
                        }

                        // สร้างคำสั่ง SQL
                        using (SqlCommand sqlCommand = new SqlCommand(strSql, conn))
                        {
                            // เพิ่มพารามิเตอร์ที่ใช้ในคำสั่ง SQL
                            sqlCommand.Parameters.AddWithValue("@movieId", lbMovieId.Text);
                            sqlCommand.Parameters.AddWithValue("@movieName", tbMovieName.Text);
                            sqlCommand.Parameters.AddWithValue("@movieDetail", tbMovieDetail.Text);
                            sqlCommand.Parameters.AddWithValue("@movieDateSale", dtpMovieDateSale.Value);
                            sqlCommand.Parameters.AddWithValue("@movieLengthHour", nudMovieHour.Value);
                            sqlCommand.Parameters.AddWithValue("@movieLengthMinute", nudMovieMinute.Value);
                            sqlCommand.Parameters.AddWithValue("@movieTypeId", cbbMovieType.SelectedIndex + 1);
                            sqlCommand.Parameters.AddWithValue("@movieDVDTotal", tbMovieDVDTotal.Text);
                            sqlCommand.Parameters.AddWithValue("@movieDVDPrice", tbMovieDVDPrice.Text);
                            sqlCommand.Parameters.AddWithValue("@movieImg", movieImg);
                            sqlCommand.Parameters.AddWithValue("@movieDirImg", movieDirImg);

                            // เริ่มต้นการทำธุรกรรม
                            SqlTransaction sqlTransaction = conn.BeginTransaction();
                            sqlCommand.Transaction = sqlTransaction;

                            try
                            {
                                // รันคำสั่ง SQL
                                sqlCommand.ExecuteNonQuery();
                                sqlTransaction.Commit(); // commit การทำธุรกรรม
                                LoadData(); // โหลดข้อมูลใหม่
                                ShareData.showWarningMSG(count > 0 ? "อัปเดตข้อมูลสำเร็จ" : "เพิ่มข้อมูลสำเร็จ");
                            }
                            catch (Exception ex)
                            {
                                // ถ้ามีข้อผิดพลาด
                                sqlTransaction.Rollback(); // ยกเลิกการทำธุรกรรม
                                ShareData.showWarningMSG("ไม่สามารถบันทึกข้อมูลได้\nError: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ถ้ามีข้อผิดพลาดในการเชื่อมต่อ DB
                        ShareData.showWarningMSG("ไม่สามารถเชื่อมต่อฐานข้อมูลได้\nError: " + ex.Message);
                    }
                }


            }
        }

        private void tbMovieDVDTotal_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void tbMovieDVDPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            if (e.KeyChar == '.' && textBox.Text.Contains('.'))
            {
                e.Handled = true;
            }
        }

        private void btEdit_Click(object sender, EventArgs e)
        {
            toggleButton(true);
            btEdit.Enabled = false;
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบข้อมูลใช่หรือไม่", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(ShareData.conStr))
                {
                    try
                    {
                        conn.Open();
                        string strSql = "DELETE FROM movie_tb WHERE movieId = @movieId";
                        using (SqlCommand cmd = new SqlCommand(strSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@movieId", lbMovieId.Text);

                            int rowsAffected = cmd.ExecuteNonQuery(); // เช็คจำนวนแถวที่ถูกลบ

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("ลบข้อมูลสำเร็จ", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบข้อมูลที่ต้องการลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

        private void btExit_Click(object sender, EventArgs e)
        {
            //ถามก่อนว่าต้องการออกจากโปรแกรมหรือไม่
            if (MessageBox.Show("ต้องการออกจากโปรแกรมใช่หรือไม่", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btMovieSearch_Click(object sender, EventArgs e)
        {
            string search = tbMovieSearch.Text.Trim(); // ดึงคำค้นจาก tbMovieSearch
            string strSql = "";

            // ถ้า tbMovieSearch ว่าง ให้แจ้งเตือนและออกจากฟังก์ชัน
            if (string.IsNullOrEmpty(search))
            {
                ShareData.showWarningMSG("กรุณากรอกข้อมูลที่ต้องการค้นหา");
                return;
            }

            // สร้างคำสั่ง SQL ตามประเภทที่เลือก
            if (rdMovieId.Checked)
            {
                // ใช้ = สำหรับ movieId เพื่อให้ตรงตัวเท่านั้น
                strSql = "SELECT m.movieId, m.movieName, m.movieDetail, m.movieDateSale, m.movieLengthHour, m.movieLengthMinute, m.movieTypeId, m.movieDVDTotal, m.movieDVDPrice, t.movieTypeName " +
                         "FROM movie_tb m " +
                         "INNER JOIN movie_type_tb t ON m.movieTypeId = t.movieTypeId " +
                         "WHERE m.movieId = @search " +
                         "ORDER BY m.movieId";
            }
            else if (rdMovieName.Checked)
            {
                // ใช้ LIKE สำหรับ movieName เพื่อให้ค้นหาแบบกว้าง
                strSql = "SELECT m.movieId, m.movieName, m.movieDetail, m.movieDateSale, m.movieLengthHour, m.movieLengthMinute, m.movieTypeId, m.movieDVDTotal, m.movieDVDPrice, t.movieTypeName " +
                         "FROM movie_tb m " +
                         "INNER JOIN movie_type_tb t ON m.movieTypeId = t.movieTypeId " +
                         "WHERE m.movieName LIKE @search " +
                         "ORDER BY m.movieId";
            }

            // ดึงข้อมูลจากฐานข้อมูลและแสดงใน ListView
            using (SqlConnection conn = new SqlConnection(ShareData.conStr))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(strSql, conn))
                    {
                        if (rdMovieId.Checked)
                        {
                            cmd.Parameters.AddWithValue("@search", search); // ค้นหา movieId ตรงตัว
                        }
                        else if (rdMovieName.Checked)
                        {
                            cmd.Parameters.AddWithValue("@search", "%" + search + "%"); // ค้นหา movieName แบบ LIKE
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // เคลียร์ข้อมูลเก่าใน ListView
                            lsMovieShow.Items.Clear();

                            // อ่านข้อมูลจากฐานข้อมูล
                            while (reader.Read())
                            {
                                // สร้าง ListViewItem
                                ListViewItem item = new ListViewItem(reader["movieId"].ToString());
                                item.SubItems.Add(reader["movieName"].ToString());
                                item.SubItems.Add(reader["movieDetail"].ToString());
                                item.SubItems.Add(Convert.ToDateTime(reader["movieDateSale"]).ToString("yyyy-MM-dd"));
                                item.SubItems.Add(reader["movieTypeName"].ToString());

                                // เพิ่ม item ลงใน ListView
                                lsMovieShow.Items.Add(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }

        }
    }
}
