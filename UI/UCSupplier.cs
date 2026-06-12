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
    public partial class UCSupplier : UserControl
    {
        private readonly BLSupplier _bus = new BLSupplier();
        private int? _currentId;
        private bool _isAdding;
        private string _err;
        public UCSupplier()
        {
            InitializeComponent();
            // DGV columns
            dgvSuppliers.AutoGenerateColumns = false;
            dgvSuppliers.Columns.Clear();
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SupplierId",
                DataPropertyName = "SupplierId",
                HeaderText = "Mã NCC",
                Width = 60
            });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SupplierName",
                DataPropertyName = "SupplierName",
                HeaderText = "Tên NCC",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvSuppliers.CellClick += dgvSuppliers_CellClick;

            LoadData();
            ResetFormState();
        }

        private void LoadData()
        {
            var listSuppliers = _bus.LayNhaCungCap().ToList(); 
            dgvSuppliers.DataSource = listSuppliers;
        }


        private void ResetFormState()
        {
            _currentId = null;
            _isAdding = false;
            txtSupplierName.Clear();

            btnAdd.Enabled = true;
            btnSave.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnSearch.Enabled = true;
            btnReload.Enabled = true;

            dgvSuppliers.Enabled = true;
            dgvSuppliers.ClearSelection();
        }

        private void dgvSuppliers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var supplier = (SupplierDTO)dgvSuppliers.Rows[e.RowIndex].DataBoundItem;

            _currentId = supplier.SupplierId;
            txtSupplierName.Text = supplier.SupplierName;

            btnAdd.Enabled = true;
            btnSave.Enabled = false;
            btnEdit.Enabled = true;
            btnDelete.Enabled = true;
        }



        private void btnAdd_Click(object sender, EventArgs e)
        {
            _isAdding = true;
            txtSupplierName.Clear();
            txtSupplierName.Focus();

            btnAdd.Enabled = false;
            btnSave.Enabled = true;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            dgvSuppliers.Enabled = false;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!_currentId.HasValue) return;
            if (MessageBox.Show("Xóa nhà cung cấp này?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (!_bus.XoaNCC(_currentId.Value, ref _err))
                MessageBox.Show("Lỗi: " + _err, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                LoadData();
                ResetFormState();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!_currentId.HasValue) return;
            _isAdding = false;
            btnAdd.Enabled = false;
            btnSave.Enabled = true;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            dgvSuppliers.Enabled = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var name = txtSupplierName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên nhà cung cấp.",
                                "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool ok;
            if (_isAdding)
                ok = _bus.ThemNCC(name, ref _err);
            else
                ok = _bus.SuaNCC(_currentId.Value, name, ref _err);

            if (!ok)
                MessageBox.Show("Lỗi: " + _err, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                LoadData();
                ResetFormState();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var keyword = txtSupplierName.Text.Trim();
            var suppliers = _bus.LayNhaCungCap();
            var result = new List<SupplierDTO>();

            foreach (var supplier in suppliers)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    result.Add(supplier);
                    continue;
                }

                string supplierName = (supplier.SupplierName ?? "").ToLower();
                if (supplierName.Contains(keyword.ToLower()))
                    result.Add(supplier);
            }

            dgvSuppliers.DataSource = result;
            dgvSuppliers.ClearSelection();
            _currentId = null;
            btnAdd.Enabled = true;
            btnSave.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            LoadData();
            ResetFormState();
        }

        private void UCSupplier_Load(object sender, EventArgs e)
        {

        }
    }
}
