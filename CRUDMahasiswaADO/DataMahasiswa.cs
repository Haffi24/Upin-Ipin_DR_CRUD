using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ExcelDataReader;

namespace CRUDMahasiswaADO
{
    public partial class DataMahasiswa : Form
    {
        DAL dbLogic = new DAL();
        private BindingSource bindingSource = new BindingSource();

        public DataMahasiswa()
        {
            InitializeComponent();
        }

        private void DataMahasiswa_Load(object sender, EventArgs e)
        {
            cmbJK.DataSource = new string[] { "L", "P" };

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            LoadData();
        }

        private void SimpanLog(string pesan)
        {
            try
            {
                dbLogic.InsertLog(pesan);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gagal simpan log: " + ex.Message);
            }
        }

        private void HitungTotal()
        {
            try
            {
                int count = dbLogic.CountMhs();
                int total = (count == 0) ? 0 : count;
                lblTotal.Text = "Total Mahasiswa: " + total;
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal menghitung total: " + ex.Message);
            }
        }

        private void LoadData()
        {
            try
            {
                bindingSource.DataSource = dbLogic.GetMhs();
                dataGridView1.DataSource = bindingSource;

                if (dataGridView1.Columns.Contains("Foto"))
                {
                    DataGridViewImageColumn fotoColumn = (DataGridViewImageColumn)dataGridView1.Columns["Foto"];
                    fotoColumn.ImageLayout = DataGridViewImageCellLayout.Stretch;
                }

                HitungTotal();

                dataGridView1.Enabled = true;
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            txtNIM.Enabled = true;
            txtNIM.ReadOnly = false;
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;

            if (fotoMhs != null)
            {
                fotoMhs.Image = null;
            }

            txtNIM.Focus();
        }

        byte[] ConvertImageToBytes(PictureBox pb)
        {
            if (pb == null || pb.Image == null) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                pb.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.CountMhs();
                MessageBox.Show("Koneksi berhasil");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi gagal: " + ex.Message);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] imgBytes = ConvertImageToBytes(fotoMhs);
                dbLogic.InsertMhs(txtNIM.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text, dtpTanggalLahir.Value.Date, txtKodeProdi.Text, imgBytes);

                SimpanLog("INSERT MAHASISWA : " + txtNIM.Text);
                MessageBox.Show("Data berhasil ditambahkan");

                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                SimpanLog("GENERAL ERROR : " + ex.Message);
                MessageBox.Show(ex.Message);
            }

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] imgBytes = ConvertImageToBytes(fotoMhs);
                dbLogic.UpdateMhs(txtNIM.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text, dtpTanggalLahir.Value.Date, txtKodeProdi.Text, imgBytes);

                MessageBox.Show("Data mahasiswa berhasil diperbarui");
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Terjadi kesalahan: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult resultConfirm = MessageBox.Show("Yakin ingin menghapus data?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resultConfirm == DialogResult.Yes)
                {
                    dbLogic.DeleteMhs(txtNIM.Text);
                    MessageBox.Show("Data berhasil dihapus");
                    ClearForm();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Terjadi kesalahan: " + ex.Message);
            }
        }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.resetData();
                MessageBox.Show("Data berhasil direset");
                LoadData();
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Reset gagal: " + ex.Message);
            }
        }

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.testInject(txtNIM.Text);
                MessageBox.Show("Test Injection berhasil dieksekusi");
                LoadData();
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataRow row = ((DataRowView)bindingSource[e.RowIndex]).Row;

                txtNIM.Text = row["NIM"].ToString();
                txtNama.Text = row["Nama"].ToString();
                cmbJK.Text = row["JenisKelamin"].ToString();

                if (row["TanggalLahir"] != DBNull.Value)
                {
                    dtpTanggalLahir.Value = Convert.ToDateTime(row["TanggalLahir"]);
                }

                txtAlamat.Text = row["Alamat"].ToString();
                txtKodeProdi.Text = row["NamaProdi"].ToString();

                if (row.Table.Columns.Contains("Foto") && row["Foto"] != DBNull.Value && fotoMhs != null)
                {
                    byte[] imgBytes = (byte[])row["Foto"];
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        fotoMhs.Image = Image.FromStream(ms);
                        fotoMhs.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else if (fotoMhs != null)
                {
                    fotoMhs.Image = null;
                }

                txtNIM.Enabled = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FormReport rekap = new FormReport();
            rekap.Show();
        }

        private void label7_Click(object sender, EventArgs e)
        {
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (fotoMhs == null)
            {
                MessageBox.Show("PictureBox dengan nama 'fotoMhs' belum ada di form!");
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fotoMhs.Image = Image.FromFile(ofd.FileName);
                fotoMhs.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void btnImpExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Excel Workbook|*.xlsx" })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                            });

                            DataTable dt = result.Tables[0];
                            dataGridView1.DataSource = dt;
                            dataGridView1.Enabled = false;

                            MessageBox.Show("Data Excel berhasil dimuat ke DataGridView. Silakan klik tombol Import ke DB.");
                        }
                    }
                }
            }
        }

        private void btnImpDb_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dataGridView1.DataSource;
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimport.");
                    return;
                }

                int sukses = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string nim = row["NIM"].ToString().Trim();
                    string nama = row["Nama"].ToString().Trim();
                    string jk = row["JenisKelamin"].ToString().Trim();
                    string alamat = row["Alamat"].ToString().Trim();

                    string kodeProdi = row.Table.Columns.Contains("NamaProdi") ? row["NamaProdi"].ToString().Trim() :
                                      (row.Table.Columns.Contains("KodeProdi") ? row["KodeProdi"].ToString().Trim() : "");

                   
                    string fotoPath = row.Table.Columns.Contains("Foto") ? row["Foto"].ToString().Trim() : string.Empty;

                    if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama)) continue;

                    DateTime tglLahir;
                    if (!DateTime.TryParse(row["TanggalLahir"].ToString(), out tglLahir))
                    {
                        tglLahir = DateTime.Now; 
                    }

                    
                    byte[] fotoBytes = null;
                    if (!string.IsNullOrWhiteSpace(fotoPath) && System.IO.File.Exists(fotoPath))
                    {
                        fotoBytes = System.IO.File.ReadAllBytes(fotoPath);
                    }

                    
                    dbLogic.InsertMhs(nim, nama, alamat, jk, tglLahir, kodeProdi, fotoBytes);
                    sukses++;
                }

                MessageBox.Show("Data mahasiswa berhasil ditambahkan: " + sukses);
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat Import DB: " + ex.Message);
            }
        }
    }
}