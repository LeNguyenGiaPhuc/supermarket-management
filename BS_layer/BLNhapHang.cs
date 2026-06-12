using System;
using System.Collections.Generic;
using System.Linq;
using Supermarket.DB_layer;

namespace Supermarket.BS_layer
{
    public class ImportOrderDTO
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal ImportPrice { get; set; }
        public int Quantity { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? IssueDate { get; set; }
    }

    public class BLNhapHang
    {
        private readonly SupermarketDBEntities db = new SupermarketDBEntities();

        /// <summary>
        /// 1. Lấy toàn bộ phiếu nhập (kèm IssueDate)
        /// </summary>
        public List<ImportOrderDTO> LayPhieuNhap()
        {
            var list = (from io in db.ImportOrders
                        join p in db.Products on io.ProductId equals p.ProductId into pp
                        from p in pp.DefaultIfEmpty()
                        join c in db.Categories on p.CategoryId equals c.CategoryId into cc
                        from c in cc.DefaultIfEmpty()
                        join s in db.Suppliers on io.SupplierId equals s.SupplierId into ss
                        from s in ss.DefaultIfEmpty()
                        orderby io.OrderId descending
                        select new ImportOrderDTO
                        {
                            OrderId = io.OrderId,
                            ProductId = io.ProductId,
                            ProductName = p != null ? p.ProductName : null,
                            CategoryName = c != null ? c.CategoryName : null,
                            SupplierId = io.SupplierId,
                            SupplierName = s != null ? s.SupplierName : null,
                            ImportPrice = io.ImportPrice,
                            Quantity = io.Quantity,
                            TotalAmount = io.TotalAmount,
                            Status = io.Status,
                            IssueDate = io.IssueDate
                        }).ToList();

            return list;
        }


        /// <summary>
        /// 2. Thêm phiếu nhập
        /// </summary>
        public bool ThemPhieuNhap(string maSP, string maNCC, string gia, string sl, ref string err)
        {
            try
            {
                ImportOrder order = new ImportOrder
                {
                    ProductId = int.Parse(maSP),
                    SupplierId = int.Parse(maNCC),
                    ImportPrice = decimal.Parse(gia),
                    Quantity = int.Parse(sl),
                    Status = "New",
                    IssueDate = null
                };

                db.ImportOrders.Add(order);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 3. Sửa phiếu nhập (chỉ khi Status = 'New')
        /// </summary>
        public bool CapNhatPhieuNhap(string maPhieu, string gia, string sl, ref string err)
        {
            try
            {
                int id = int.Parse(maPhieu);
                var order = db.ImportOrders.FirstOrDefault(o => o.OrderId == id && o.Status == "New");

                if (order == null)
                {
                    err = "Không tìm thấy phiếu nhập hoặc trạng thái không hợp lệ.";
                    return false;
                }

                if (order != null)
                {
                    order.ImportPrice = decimal.Parse(gia);
                    order.Quantity = int.Parse(sl);
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 4. Xóa phiếu nhập (chỉ khi Status = 'New')
        /// </summary>
        public bool XoaPhieuNhap(string maPhieu, ref string err)
        {
            try
            {
                int id = int.Parse(maPhieu);
                var order = db.ImportOrders.FirstOrDefault(o => o.OrderId == id && o.Status == "New");

                if (order == null)
                {
                    err = "Không tìm thấy phiếu nhập hoặc trạng thái không hợp lệ.";
                    return false;
                }

                if (order != null)
                {
                    db.ImportOrders.Remove(order);
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 5. Xuất phiếu (New → Issued) và ghi IssueDate
        /// </summary>
        public bool XuatPhieu(string maPhieu, DateTime ngayXuat, ref string err)
        {
            try
            {
                int id = int.Parse(maPhieu);
                var order = db.ImportOrders.FirstOrDefault(o => o.OrderId == id && o.Status == "New");

                if (order == null)
                {
                    err = "Không tìm thấy phiếu nhập hoặc trạng thái không hợp lệ.";
                    return false;
                }

                if (order != null)
                {
                    order.Status = "Issued";
                    order.IssueDate = ngayXuat;
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }
    }
}

