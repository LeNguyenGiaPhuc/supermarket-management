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

namespace Supermarket
{
    public partial class UCKho : UserControl
    {
        private int? _currentProductId = null;
        private BLSanPham dbSP = new BLSanPham();
        private BLCategory catBus = new BLCategory();
        private BLSupplier supBus = new BLSupplier();
        private bool isAdding;
        private string err;

        public UCKho()
        {
            InitializeComponent();
            dgvSP.MultiSelect = false;
            dgvSP.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSP.SelectionChanged += dgvSP_SelectionChanged;
            LoadCombos();
            LoadData();
        }


        private void ClearInputs()
        {
            _currentProductId = null;

            txtTenSP.Clear();
            cmbCategory.SelectedIndex = -1;
            cmbSupplier.SelectedIndex = -1;
            dtpExpiry.Value = DateTime.Today;
        }

        private void LoadCombos()
        {
            var categories = catBus.GetAll().ToList(); 
            cmbCategory.DataSource = categories;
            cmbCategory.DisplayMember = "CategoryName";
            cmbCategory.ValueMember = "CategoryId";
            cmbCategory.SelectedIndex = -1;

            var suppliers = supBus.LayNhaCungCap().ToList();
            cmbSupplier.DataSource = suppliers;
            cmbSupplier.DisplayMember = "SupplierName";
            cmbSupplier.ValueMember = "SupplierId";
            cmbSupplier.SelectedIndex = -1;
        }



        private void LoadData()
        {
            var products = dbSP.LaySanPham().ToList();
            dgvSP.DataSource = products;

            // Ẩn cột nếu cần
            dgvSP.Columns["CategoryId"].Visible = false;
            dgvSP.Columns["SupplierId"].Visible = false;

            cmbSupplier.Enabled = true;
            ClearInputs();
            SetButtonState(false);
        }


        private void SetButtonState(bool isEditing)
        {
            btnThem.Enabled = !isEditing;
            bool hasSel = _currentProductId.HasValue;
            btnSua.Enabled = !isEditing && hasSel;
            btnXoa.Enabled = !isEditing && hasSel;
            btnReload.Enabled = !isEditing;
            btnLuu.Enabled = isEditing;
            dgvSP.Enabled = !isEditing;
        }


        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true;
            ClearInputs();
            SetButtonState(isEditing: true);

            cmbSupplier.Enabled = false;
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (!_currentProductId.HasValue) return;
            isAdding = false;
            SetButtonState(isEditing: true);

            cmbSupplier.Enabled = true;
        }


        private void btnSearch_Click(object sender, EventArgs e)
        {
            var products = dbSP.LaySanPham().ToList();
            var result = new List<ProductDTO>();

            string keyword = txtTenSP.Text.Trim().ToLower();
            int? categoryId = null;
            int? supplierId = null;
            DateTime? expiryDate = null;

            if (cmbCategory.SelectedIndex >= 0 && cmbCategory.SelectedValue != null)
                categoryId = Convert.ToInt32(cmbCategory.SelectedValue);

            if (cmbSupplier.SelectedIndex >= 0 && cmbSupplier.SelectedValue != null)
                supplierId = Convert.ToInt32(cmbSupplier.SelectedValue);

            if (chkUseExpiry.Checked)
                expiryDate = dtpExpiry.Value.Date;

            foreach (var product in products)
            {
                bool matched = true;

                if (!string.IsNullOrEmpty(keyword))
                {
                    string productName = (product.ProductName ?? "").ToLower();
                    if (!productName.Contains(keyword))
                        matched = false;
                }

                if (categoryId.HasValue && product.CategoryId != categoryId.Value)
                    matched = false;

                if (supplierId.HasValue && product.SupplierId != supplierId.Value)
                    matched = false;

                if (expiryDate.HasValue)
                {
                    if (!product.ExpiryDate.HasValue || product.ExpiryDate.Value.Date != expiryDate.Value)
                        matched = false;
                }

                if (matched)
                    result.Add(product);
            }

            dgvSP.DataSource = result;

            if (dgvSP.Columns.Contains("CategoryId"))
                dgvSP.Columns["CategoryId"].Visible = false;

            if (dgvSP.Columns.Contains("SupplierId"))
                dgvSP.Columns["SupplierId"].Visible = false;
        }


        private void btnReload_Click(object sender, EventArgs e)
        {
            LoadData();
            isAdding = false;
            ClearInputs();
            SetButtonState(isEditing: false);
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (!_currentProductId.HasValue) return;
            if (MessageBox.Show("Xóa sản phẩm này?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            bool ok = dbSP.XoaSanPham(_currentProductId.Value, ref err);
            if (!ok) MessageBox.Show("Lỗi xóa: " + err);
            else
            {
                MessageBox.Show("Đã xóa.");
                LoadData();
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenSP.Text) || cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng nhập Tên sản phẩm và Chọn Danh mục.",
                                "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tenSP = txtTenSP.Text.Trim();

            // Chuyển SelectedValue sang int
            int maDM = Convert.ToInt32(cmbCategory.SelectedValue);

            // Nhà cung cấp có thể chưa chọn (null), xử lý nullable int
            int? maNCC = null;
            if (cmbSupplier.SelectedValue != null)
            {
                maNCC = Convert.ToInt32(cmbSupplier.SelectedValue);
            }

            // Lấy ngày hết hạn từ DateTimePicker (DateTime?)
            DateTime? hanD = dtpExpiry.Value;

            bool success;
            string err = "";

            if (isAdding)
            {
                success = dbSP.ThemSanPham(tenSP, maDM, maNCC, hanD, ref err);
                if (!success)
                {
                    MessageBox.Show("Lỗi thêm: " + err, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                if (!_currentProductId.HasValue)
                {
                    MessageBox.Show("Không có sản phẩm để cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (cmbSupplier.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn Nhà cung cấp.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                success = dbSP.CapNhatSanPham(
                    _currentProductId.Value,
                    tenSP, maDM, maNCC, hanD,
                    ref err
                );
                if (!success)
                {
                    MessageBox.Show("Lỗi sửa: " + err, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            MessageBox.Show("Thao tác thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadData();
        }


        private void dgvSP_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSP.SelectedRows.Count == 0)
            {
                _currentProductId = null;
                ClearInputs();
                SetButtonState(false);
                return;
            }
            var row = dgvSP.SelectedRows[0];
            _currentProductId = Convert.ToInt32(row.Cells["ProductId"].Value);
            txtTenSP.Text = row.Cells["ProductName"].Value?.ToString() ?? "";
            cmbCategory.SelectedValue = (int)row.Cells["CategoryId"].Value;
            cmbSupplier.Enabled = true;
            var supVal = row.Cells["SupplierId"].Value;
            if (supVal != null && supVal != DBNull.Value)
                cmbSupplier.SelectedValue = Convert.ToInt32(supVal);
            else
                cmbSupplier.SelectedIndex = -1;

            var expVal = row.Cells["ExpiryDate"].Value;
            dtpExpiry.Value = expVal != null && expVal != DBNull.Value
                ? (DateTime)expVal
                : DateTime.Today;

            SetButtonState(false);
        }



        private void UCKho_Load(object sender, EventArgs e)
        {

        }

        private void chkUseExpiry_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
