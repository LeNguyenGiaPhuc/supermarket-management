using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Supermarket.BS_layer;
using Supermarket.DB_layer;

namespace Supermarket
{
    public partial class UCEmployee : UserControl
    {
        private readonly BLNhanVien _bus = new BLNhanVien();
        private int? _currentId;
        private bool _isAdding;
        private string _err;

        public UCEmployee()
        {
            InitializeComponent();

            // DataGridView setup
            dgvNV.MultiSelect = false;
            dgvNV.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvNV.AutoGenerateColumns = false;
            dgvNV.Columns.Clear();

            dgvNV.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EmployeeId",
                DataPropertyName = "EmployeeId",
                HeaderText = "Mã NV",
                Width = 60
            });
            dgvNV.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                DataPropertyName = "FullName",
                HeaderText = "Tên NV",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dgvNV.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Phone",
                DataPropertyName = "Phone",
                HeaderText = "SĐT",
                Width = 120
            });
            dgvNV.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Salary",
                DataPropertyName = "Salary",
                HeaderText = "Lương",
                Width = 80
            });
            dgvNV.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HireDate",
                DataPropertyName = "HireDate",
                HeaderText = "Ngày vào làm",
                Width = 110
            });

            dgvNV.CellClick += dgvNV_CellClick;

            LoadData();
            ResetForm();
        }

        private void LoadData()
        {
            var danhSach = _bus.LayNhanVien(); // List<EmployeeDTO>
            dgvNV.DataSource = danhSach;
        }



        private void ResetForm()
        {
            _currentId = null;
            _isAdding = false;
            txtName.Clear();
            txtPhone.Clear();
            txtSalary.Clear();
            chkUseHireDate.Checked = false;
            dtpHireDate.Value = DateTime.Today;

            btnAdd.Enabled = true;
            btnSave.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnSearch.Enabled = true;
            btnClear.Enabled = true;
            dgvNV.Enabled = true;
            dgvNV.ClearSelection();
        }

        private void dgvNV_CellClick(object s, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var emp = (EmployeeDTO)dgvNV.Rows[e.RowIndex].DataBoundItem;
            _currentId = emp.EmployeeId;
            txtName.Text = emp.FullName;
            txtPhone.Text = emp.Phone;
            txtSalary.Text = emp.Salary.ToString();
            dtpHireDate.Value = emp.HireDate;

            btnAdd.Enabled = true;
            btnSave.Enabled = false;
            btnEdit.Enabled = true;
            btnDelete.Enabled = true;
        }



        private void btnAdd_Click(object sender, EventArgs e)
        {
            _isAdding = true;
            txtName.Clear();
            txtPhone.Clear();
            txtSalary.Clear();
            dtpHireDate.Value = DateTime.Today;

            btnAdd.Enabled = false;
            btnSave.Enabled = true;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            dgvNV.Enabled = false;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!_currentId.HasValue) return;
            _isAdding = false;

            btnAdd.Enabled = false;
            btnSave.Enabled = true;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            dgvNV.Enabled = false;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!_currentId.HasValue) return;
            if (MessageBox.Show("Xóa nhân viên này?",
                   "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes) return;

            if (!_bus.XoaNhanVien(_currentId.Value.ToString(), ref _err))
                MessageBox.Show("Lỗi: " + _err,
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                LoadData();
                ResetForm();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        { 
            if (string.IsNullOrWhiteSpace(txtName.Text)
             || string.IsNullOrWhiteSpace(txtPhone.Text)
             || string.IsNullOrWhiteSpace(txtSalary.Text))
            {
                MessageBox.Show("Nhập đủ Tên, SĐT và Lương.",
                                "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtName.Text.Trim()
                               .Replace("'", "''");
            string phone = txtPhone.Text.Trim()
                                 .Replace("'", "''");
            string salary = txtSalary.Text.Trim();
            string hireDate = dtpHireDate.Value.ToString("yyyy-MM-dd");

            bool ok;
            if (_isAdding)
            {
                ok = _bus.ThemNhanVien(name, phone, salary, hireDate, ref _err);
            }
            else
            {
                ok = _bus.SuaNhanVien(
                    _currentId.Value.ToString(),
                    name, phone, salary, hireDate,
                    ref _err);
            }

            if (!ok)
                MessageBox.Show("Lỗi: " + _err,
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                LoadData();
                ResetForm();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var employees = _bus.LayNhanVien();
            var result = new List<EmployeeDTO>();

            string name = txtName.Text.Trim().ToLower();
            string phone = txtPhone.Text.Trim().ToLower();
            bool useHireDate = chkUseHireDate.Checked;
            DateTime hireDate = dtpHireDate.Value.Date;

            foreach (var employee in employees)
            {
                bool matched = true;

                if (!string.IsNullOrEmpty(name))
                {
                    string employeeName = (employee.FullName ?? "").ToLower();
                    if (!employeeName.Contains(name))
                        matched = false;
                }

                if (!string.IsNullOrEmpty(phone))
                {
                    string employeePhone = (employee.Phone ?? "").ToLower();
                    if (!employeePhone.Contains(phone))
                        matched = false;
                }

                if (useHireDate && employee.HireDate.Date != hireDate)
                    matched = false;

                if (matched)
                    result.Add(employee);
            }

            dgvNV.DataSource = result;
            dgvNV.ClearSelection();
            _currentId = null;
            btnAdd.Enabled = true;
            btnSave.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            LoadData();
            ResetForm();
            chkUseHireDate.Checked = false;
        }

        private void UCEmployee_Load(object sender, EventArgs e)
        {
             
        }
    }
}
