using System;
using System.Linq;
using System.Windows.Forms;
using Supermarket.BS_layer;
using System.Speech.Synthesis;
using System.Data;





namespace Supermarket
{
    public partial class UCNhapHang : UserControl
    {
        private readonly SpeechSynthesizer _tts = new SpeechSynthesizer();

        // BUS layer
        private BLNhapHang _ioBus;
        private BLCategory _catBus;
        private BLSupplier _supBus;

        // nội bộ
        private int? _currentId;
        private bool _isAdding;
        private string _err;

        public UCNhapHang()
        {
            InitializeComponent();
            nudQuantity.Minimum = 1;
            nudQuantity.Maximum = 10000;

            // 1) Khởi instance BUS
            _ioBus = new BLNhapHang();
            _catBus = new BLCategory();
            _supBus = new BLSupplier();

            // 2) Thiết lập DataGridView
            dgvImportOrders.MultiSelect = false;
            dgvImportOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvImportOrders.AutoGenerateColumns = false;
            InitGridColumns();

            // 3) Load dữ liệu
            LoadCombos();
            LoadImportOrders();
            ResetFormState();

            // 4) Gán event
            dgvImportOrders.SelectionChanged += dgvImportOrders_SelectionChanged;

            _tts.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            _tts.Rate = 0;      // -10 (chậm) → +10 (nhanh)
            _tts.Volume = 100;
        }

        private void InitGridColumns()
        {
            dgvImportOrders.Columns.Clear();

            dgvImportOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProductId",
                DataPropertyName = "ProductId",
                Visible = false
            });
            dgvImportOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SupplierId",
                DataPropertyName = "SupplierId",
                Visible = false
            });

            

            void AddCol(string prop, string header)
            {
                dgvImportOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = prop,
                    HeaderText = header,
                    Name = prop
                });
            }

            AddCol("OrderId", "Mã phiếu");
            AddCol("ProductName", "Sản phẩm");
            AddCol("SupplierName", "Nhà cung cấp");
            AddCol("CategoryName", "Danh mục");
            AddCol("ImportPrice", "Giá nhập");
            AddCol("Quantity", "Số lượng");
            AddCol("TotalAmount", "Thành tiền");
            AddCol("Status", "Trạng thái");
            AddCol("IssueDate", "Ngày xuất phiếu");

            dgvImportOrders.Columns["IssueDate"].Visible = true;
            dgvImportOrders.Columns["IssueDate"].HeaderText = "Ngày xuất phiếu";
        }

        private void LoadCombos()
        {
            // Sản phẩm
            var dsP = new BLSanPham().LaySanPham().ToList(); // IQueryable → List
            cmbProduct.DataSource = dsP;
            cmbProduct.DisplayMember = "ProductName";
            cmbProduct.ValueMember = "ProductId";
            cmbProduct.SelectedIndex = -1;

            // Nhà cung cấp
            var dsS = _supBus.LayNhaCungCap().ToList(); // IQueryable → List
            cmbSupplier.DataSource = dsS;
            cmbSupplier.DisplayMember = "SupplierName";
            cmbSupplier.ValueMember = "SupplierId";
            cmbSupplier.SelectedIndex = -1;
        }


        private void LoadImportOrders()
        {
            var dsIO = _ioBus.LayPhieuNhap(); // List<dynamic>
            dgvImportOrders.DataSource = dsIO;

            LoadCombos();
        }


        private void ClearInputFields()
        {
            _currentId = null;
            cmbProduct.SelectedIndex = -1;
            cmbSupplier.SelectedIndex = -1;
            nudQuantity.Value = 1;
            txtPrice.Clear();
        }

        private void ResetFormState()
        {
            btnCreate.Enabled = true;
            btnSave.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnIssue.Enabled = false;
            btnReload.Enabled = true;
            dgvImportOrders.Enabled = true;
            ClearInputFields();
        }


        private void dgvImportOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvImportOrders.SelectedRows.Count == 0)
            {
                ResetFormState();
                return;
            }

            // Ép kiểu 
            var selectedOrder = dgvImportOrders.SelectedRows[0].DataBoundItem as ImportOrderDTO;
            if (selectedOrder == null)
            {
                ResetFormState();
                return;
            }

            // Lấy các thuộc tính từ DTO
            _currentId = selectedOrder.OrderId;
            int prodId = selectedOrder.ProductId;
            int supId = selectedOrder.SupplierId;

            // Gán vào combo theo ID
            cmbProduct.SelectedValue = prodId;
            cmbSupplier.SelectedValue = supId;

            // Phần còn lại
            txtPrice.Text = selectedOrder.ImportPrice.ToString();
            int quantity = selectedOrder.Quantity;
            if (quantity < nudQuantity.Minimum)
                quantity = (int)nudQuantity.Minimum;
            if (quantity > nudQuantity.Maximum)
                quantity = (int)nudQuantity.Maximum;
            nudQuantity.Value = quantity;

            // nút
            bool canModify = selectedOrder.Status == "New";
            btnCreate.Enabled = true;
            btnSave.Enabled = false;
            btnEdit.Enabled = canModify;
            btnDelete.Enabled = canModify;
            btnIssue.Enabled = canModify;
        }





        private void btnCreate_Click(object sender, EventArgs e)
        {
            _isAdding = true;
            ClearInputFields();
            btnCreate.Enabled = false;
            btnSave.Enabled = true;
            dgvImportOrders.Enabled = false;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!_currentId.HasValue) return;
            _isAdding = false;
            btnSave.Enabled = true;
            btnCreate.Enabled = false;
            dgvImportOrders.Enabled = false;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!_currentId.HasValue) return;
            if (MessageBox.Show("Xóa phiếu này?", "", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            if (!_ioBus.XoaPhieuNhap(_currentId.Value.ToString(), ref _err))
                MessageBox.Show(_err);
            else
            {
                LoadImportOrders();
                ResetFormState();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedValue == null || cmbSupplier.SelectedValue == null)
            {
                MessageBox.Show("Chọn sản phẩm và nhà cung cấp.");
                return;
            }

            var pid = cmbProduct.SelectedValue.ToString();
            var sid = cmbSupplier.SelectedValue.ToString();
            var prodName = cmbProduct.Text;    
            var supName = cmbSupplier.Text;     
            var pr = txtPrice.Text;
            var qty = nudQuantity.Value.ToString();
            bool ok;

            if (_isAdding)
                ok = _ioBus.ThemPhieuNhap(pid, sid, pr, qty, ref _err);
            else
                ok = _ioBus.CapNhatPhieuNhap(_currentId.Value.ToString(), pr, qty, ref _err);

            if (!ok)
            {
                MessageBox.Show(_err);
            }
            else
            {
                LoadImportOrders();
                ResetFormState();

                // → Đọc voice thông báo
                string msg = _isAdding
                    ? $"A new import order was successfully created."
                    : $"A import order has just been successfully updated";
                _tts.SpeakAsync(msg);
            }
        }


        private void btnIssue_Click(object sender, EventArgs e)
        {
            if (dgvImportOrders.CurrentRow == null) return;
            // Lấy OrderId
            int id = (int)dgvImportOrders.CurrentRow.Cells["OrderId"].Value;

            // Hiển thị dialog để chọn ngày xuất
            if (!ShowIssueDateDialog(out DateTime ngayXuat))
                return;

            string err = "";
            if (_ioBus.XuatPhieu(id.ToString(), ngayXuat, ref err))
            {
                // Load lại và reset form
                MessageBox.Show("Xuất phiếu thành công.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadImportOrders();
                ResetFormState();

                // --- Voice feedback ---
                string msg = $"A import receipt has just been successfully exported, quantity will be updated soon, please wait " ;
                _tts.SpeakAsync(msg);
            }
            else
            {
                MessageBox.Show("Chỉ xuất được khi Status = New.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }




        private bool ShowIssueDateDialog(out DateTime selectedDate)
        {
            selectedDate = DateTime.Now;

            using (var dlg = new Form())
            {
                dlg.Text = "Chọn ngày xuất";
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

                // Hiện dialog
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    selectedDate = dtp.Value;
                    return true;
                }
                return false;
            }
        }




        private void dgvImportOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void btnReload_Click(object s, EventArgs e)
        {
            LoadImportOrders();
            ResetFormState();
        }

        private void UCImportOrders_Load(object sender, EventArgs e)
        {

        }
    }
}
