using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class FormCetak : Form
    {
        private string prodiDipilih;
        private DateTime tahunDipilih;
        private bool pakaiFilter;

        public FormCetak()
        {
            InitializeComponent();
            this.pakaiFilter = false;
        }

        public FormCetak(string prodi, DateTime tahun)
        {
            InitializeComponent();
            this.prodiDipilih = prodi;
            this.tahunDipilih = tahun;
            this.pakaiFilter = true;
        }

        private void FormCetak_Load(object sender, EventArgs e)
        {
            string connString = @"Data Source=LAPTOP-D3717QUD\USERHAFFI; Initial Catalog=DBAkademikADO; Integrated Security=True";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    SqlCommand cmd;

                    if (pakaiFilter)
                    {
                        cmd = new SqlCommand("sp_Report", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@inProdi", prodiDipilih);
                        cmd.Parameters.AddWithValue("@inTglMsuk", tahunDipilih.Year.ToString());
                    }
                    else
                    {
                        cmd = new SqlCommand("SELECT * FROM Mahasiswa", conn);
                        cmd.CommandType = CommandType.Text;
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();

                    da.Fill(ds, "Mahasiswa");

                    ListMahasiswa rpt = new ListMahasiswa();
                    rpt.SetDataSource(ds.Tables["Mahasiswa"]);

                    crystalReportViewer2.ReportSource = rpt;
                    crystalReportViewer2.Refresh();
                }
                catch (Exception ex)
                {
                    
                    string pesanError = "Gagal memuat laporan:\n" + ex.Message;

                    if (ex.InnerException != null)
                    {
                        pesanError += "\n\nDetail Asli (Inner Exception):\n" + ex.InnerException.Message;
                    }

                    MessageBox.Show(pesanError, "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void crystalReportViewer2_Load(object sender, EventArgs e)
        {

        }
    }
}