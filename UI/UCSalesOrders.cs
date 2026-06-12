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
    public partial class UCInvoice : UserControl
    {
        private BLSalesOrder _soBus;
        private BLCategory _catBus;
        private BLSupplier _supBus;
        private BLSanPham _prodBus;

        private int? _currentSOId;
        private bool _isAdding;
        private string _err;


        public UCInvoice()
        {
            InitializeComponent();

            // 1) Khởi instance BUS
            _soBus = new BLSalesOrder();
            _catBus = new BLCategory();
            _supBus = new BLSupplier();
            _prodBus = new BLSanPham();

            // 2) DGV settings
            dgvInvoices.MultiSelect = false;
            dgvInvoices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInvoices.AutoGenerateColumns = false;
            InitInvoiceGrid();

            // 3) Load combo + data + reset
            LoadInvoiceCombos();
            LoadInvoices();
            ResetInvoiceForm();

            // 4) Events
            dgvInvoices.CellClick += dgvInvoices_CellClick;
            txtInvoicePrice.TextChanged += InvoiceInputChanged;
            nudInvoiceQty.ValueChanged += InvoiceInputChanged;
        }

        private void InitInvoiceGrid()
        {
            dgvInvoices.Columns.Clear();
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProductId",
                DataPropertyName = "ProductId",
                Visible = false
            });

            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OrderId",
                DataPropertyName = "OrderId",
                HeaderText = "Mã hóa đơn"
            });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProductName",
                DataPropertyName = "ProductName",
                HeaderText = "Sản phẩm"
            });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CategoryName",
                DataPropertyName = "CategoryName",
                HeaderText = "Danh mục"
            });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SellPrice",
                DataPropertyName = "SellPrice",
                HeaderText = "Giá bán"
            });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                DataPropertyName = "Quantity",
                HeaderText = "Số lượng"
            });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalAmount",
                DataPropertyName = "TotalAmount",
                HeaderText = "Thành tiền"
            });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                DataPropertyName = "Status",
                HeaderText = "Trạng thái"
            });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IssueDate",
                DataPropertyName = "IssueDate",
                HeaderText = "Ngày xuất",
                Width = 120
            });

            // bật cột IssueDate
            dgvInvoices.Columns["IssueDate"].Visible = true;
            dgvInvoices.Columns["IssueDate"].HeaderText = "Ngày xuất";
            // (tùy chọn) đặt lại vị trí cột
            dgvInvoices.Columns["IssueDate"].DisplayIndex =
                dgvInvoices.Columns["Status"].DisplayIndex + 1;


        }

        private void LoadInvoiceCombos()
        {
            // Lấy danh sách sản phẩm dạng List từ IQueryable
            var listProducts = _prodBus.LaySanPham().ToList();

            cmbInvoiceProduct.DataSource = listProducts;
            cmbInvoiceProduct.DisplayMember = "ProductName";
            cmbInvoiceProduct.ValueMember = "ProductId";
            cmbInvoiceProduct.SelectedIndex = -1;

            // Categories nếu trả về DataTable thì giữ nguyên
            var dtC = _catBus.GetAll();
            cmbInvoiceCategory.DataSource = dtC;
            cmbInvoiceCategory.DisplayMember = "CategoryName";
            cmbInvoiceCategory.ValueMember = "CategoryId";
            cmbInvoiceCategory.SelectedIndex = -1;
        }




        private void LoadInvoices()
        {
            var invoices = _soBus.LayHoaDon().ToList();
            dgvInvoices.DataSource = invoices;
            ResetInvoiceForm();
        }


        private void ResetInvoiceForm()
        {
            _currentSOId = null;
            _isAdding = false;

            cmbInvoiceProduct.SelectedIndex = -1;
            cmbInvoiceCategory.SelectedIndex = -1;
            txtInvoicePrice.Clear();
            nudInvoiceQty.Minimum = 1;
            nudInvoiceQty.Maximum = 10000;
            nudInvoiceQty.Value = 1;
            lblInvoiceTotal.Text = "0";

            btnInvoiceCreate.Enabled = true;
            btnInvoiceSave.Enabled = false;
            btnInvoiceEdit.Enabled = false;
            btnInvoiceDelete.Enabled = false;
            btnInvoiceIssue.Enabled = false;
            btnInvoiceReload.Enabled = true;
            dgvInvoices.Enabled = true;
        }

        private void UpdateInvoiceTotal()
        {
            if (decimal.TryParse(txtInvoicePrice.Text, out var price))
                lblInvoiceTotal.Text = (price * nudInvoiceQty.Value).ToString("N0");
            else
                lblInvoiceTotal.Text = "0";
        }

        private void InvoiceInputChanged(object sender, EventArgs e)
        {
            UpdateInvoiceTotal();
        }

        private void dgvInvoices_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Nếu click vào header hoặc ngoài hàng dữ liệu thì bỏ qua
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var invoice = dgvInvoices.Rows[e.RowIndex].DataBoundItem as SalesOrderDTO;
            if (invoice == null) return;

            // Lấy ID và bind lên form
            _currentSOId = invoice.OrderId;
            int prodId = invoice.ProductId;

            cmbInvoiceProduct.SelectedValue = prodId;
            txtInvoicePrice.Text = invoice.SellPrice.ToString();
            int quantity = invoice.Quantity;
            if (quantity < nudInvoiceQty.Minimum)
                quantity = (int)nudInvoiceQty.Minimum;
            if (quantity > nudInvoiceQty.Maximum)
                quantity = (int)nudInvoiceQty.Maximum;
            nudInvoiceQty.Value = quantity;
            lblInvoiceTotal.Text = (invoice.TotalAmount ?? 0).ToString("N0");

            // Bật/tắt nút phù hợp
            bool canModify = invoice.Status == "New";
            btnInvoiceCreate.Enabled = true;
            btnInvoiceSave.Enabled = false;
            btnInvoiceEdit.Enabled = canModify;
            btnInvoiceDelete.Enabled = canModify;
            btnInvoiceIssue.Enabled = canModify;
        }


        private void UCSalesOrders_Load(object sender, EventArgs e)
        {

        }

        private void btnInvoiceCreate_Click(object sender, EventArgs e)
        {
            _isAdding = true;
            cmbInvoiceProduct.SelectedIndex = -1;
            txtInvoicePrice.Clear();
            nudInvoiceQty.Value = 1;
            btnInvoiceCreate.Enabled = false;
            btnInvoiceSave.Enabled = true;
            dgvInvoices.Enabled = false;
        }

        private void btnInvoiceEdit_Click(object sender, EventArgs e)
        {
            if (!_currentSOId.HasValue) return;
            _isAdding = false;
            btnInvoiceSave.Enabled = true;
            btnInvoiceCreate.Enabled = false;
            dgvInvoices.Enabled = false;
        }

        private void btnInvoiceDelete_Click(object sender, EventArgs e)
        {
            if (!_currentSOId.HasValue) return;
            if (MessageBox.Show("Xóa hóa đơn?", "", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            if (!_soBus.XoaHoaDon(_currentSOId.Value, ref _err))
                MessageBox.Show(_err);
            else
            {
                LoadInvoices();
                ResetInvoiceForm();
            }
        }


        private void btnInvoiceSave_Click(object sender, EventArgs e)
        {
            if (cmbInvoiceProduct.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm.", "Cảnh báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int prodId = (int)cmbInvoiceProduct.SelectedValue;
            int reqQty = (int)nudInvoiceQty.Value;
            int inStock = _prodBus.GetQuantity(prodId);

            if (reqQty > inStock)
            {
                MessageBox.Show(
                  $"Không thể tạo hóa đơn.\nTồn kho thực tế chỉ còn {inStock} sản phẩm.",
                  "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtInvoicePrice.Text, out decimal price))
            {
                MessageBox.Show("Giá không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool ok;
            if (_isAdding)
                ok = _soBus.ThemHoaDon(prodId, price, reqQty, ref _err);
            else
                ok = _soBus.CapNhatHoaDon(_currentSOId.Value, price, reqQty, ref _err);

            if (!ok)
                MessageBox.Show("Lỗi: " + _err, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                LoadInvoices();
                ResetInvoiceForm();
            }
        }


        private void btnInvoiceIssue_Click(object sender, EventArgs e)
        {
            if (!_currentSOId.HasValue) return;

            // 1) Hiện dialog chọn ngày
            if (!ShowIssueDateDialog(out var ngayXuat))
                return;   // user bấm Cancel

            // 2) Gọi BLL
            string err = "";
            if (_soBus.XuatHoaDon(_currentSOId.Value, ngayXuat, ref err))
            {
                MessageBox.Show("Xuất hóa đơn thành công.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadInvoices();       // reload lại grid
                ResetInvoiceForm(); // reset nút, ô nhập
            }
            else
            {
                MessageBox.Show("Lỗi: " + err,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private bool ShowIssueDateDialog(out DateTime selectedDate)
        {
            selectedDate = DateTime.Now;
            using (var dlg = new Form())
            {
                dlg.Text = "Chọn ngày xuất hóa đơn";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Width = 260;
                dlg.Height = 120;

                var dtp = new DateTimePicker
                {
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "yyyy-MM-dd HH:mm:ss",
                    Dock = DockStyle.Top
                };
                dlg.Controls.Add(dtp);

                var btnOk = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Dock = DockStyle.Bottom,
                    Height = 30
                };
                dlg.Controls.Add(btnOk);
                dlg.AcceptButton = btnOk;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    selectedDate = dtp.Value;
                    return true;
                }
                return false;
            }
        }


        private void btnInvoiceReload_Click(object sender, EventArgs e)
        {
            LoadInvoices();
            ResetInvoiceForm();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
