using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using global::sln_SingleApartment.Models;
using Newtonsoft.Json;
using sln_SingleApartment.ViewModels;
using PagedList;
using sln_SingleApartment.ViewModel;

namespace sln_SingleApartment.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        #region index.html
        public ActionResult Home()
        {
            SingleApartmentEntities db = new SingleApartmentEntities();
            List<CProductViewModel> list = new List<CProductViewModel>();
            foreach(var item in db.Product.Where(r => r.Discontinued == "N" && r.Stock >= 0))
            {
                list.Add(new CProductViewModel(){ entity = item });
            }
            
            return View(list);
        }
        #endregion
        public ActionResult test()
        {
            return View();
        }
        #region shop.cshtml
        public ActionResult shop()
        {
            SingleApartmentEntities db = new SingleApartmentEntities();
            List<CProductViewModel> list_product = new List<CProductViewModel>();
            foreach (var item in db.Product.Where(r => r.Discontinued== "N" && r.Stock >= 0)) 
            {
                list_product.Add(new CProductViewModel() { entity = item });
            }
            
            List<CProductMainCategoryViewModel> list_main = new List<CProductMainCategoryViewModel>();
            foreach (var item in db.ProductMainCategory)
            {
                list_main.Add(new CProductMainCategoryViewModel() {
                    entity_MainCategory = item,
                    ProductCount = db.Product.Where(r=>r.ProductSubCategory.ProductMainCategoryID==item.ProductMainCategoryID).Count()
                });
            }
            List<CProductSubCategoryViewModel> list_sub = new List<CProductSubCategoryViewModel>();
            foreach (var item in db.ProductSubCategory)
            {
                list_sub.Add(new CProductSubCategoryViewModel()
                {
                    entity_SubCategory=  item,
                    ProductCount = db.Product.Where(r=>r.ProductSubCategoryID==item.ProductSubCategoryID).Count()
                });
            }

            ShopViewModel mymodel = new ShopViewModel() { product = list_product, MainCategory = list_main, SubCategory=list_sub };

            return View(mymodel);
        }

        public JsonResult GetProductShowing (string condition, string id)
        {
            SingleApartmentEntities db = new SingleApartmentEntities();
            List<CProductViewModel> list_product = new List<CProductViewModel>();
            if (condition.Trim() == "all")
            {
                foreach (var item in db.Product.Where(r => r.Discontinued == "N" && r.Stock >= 0))
                {
                    list_product.Add(new CProductViewModel() { entity = item });
                }
            }
            else if (condition=="SubCategory" && id!=null)
            {
                foreach (var item in db.Product.Where(r => r.Discontinued == "N" && r.Stock >= 0 &&r.ProductSubCategoryID.ToString()==id))
                {
                    list_product.Add(new CProductViewModel() { entity = item });
                }
            }
            else if(condition=="MainCategory" &&id!= null){
                foreach (var item in db.Product.Where(r => r.Discontinued == "N" && r.Stock >= 0 && r.ProductSubCategory.ProductMainCategoryID.ToString() == id))
                {
                    list_product.Add(new CProductViewModel() { entity = item });
                }
            }
            List<string> ArrayList = new List<string>();
            foreach (var item in list_product)
            {
                var json = JsonConvert.SerializeObject(item);
                ArrayList.Add(json);
            }
            return Json(ArrayList);
        }
        #endregion
       
        public JsonResult AddToFavorite(string MemberID, string ProductID)
        {
            SingleApartmentEntities db = new SingleApartmentEntities();
            var fa = db.FavoriteList.Where(r => r.MemberID.ToString() == MemberID && r.ProductID.ToString() == ProductID);
            if (fa.Count()==0)
            {
                FavoriteList fv = new FavoriteList() { MemberID = int.Parse(MemberID), ProductID = int.Parse(ProductID) };
                db.FavoriteList.Add(fv);
                db.SaveChanges();
                var json = JsonConvert.SerializeObject(new { ans = "已成功加入我的最愛" });
                return Json(json);
            }
            else return Json(JsonConvert.SerializeObject(new { ans = "我的最愛裡已有此件商品" }));
        }

        #region FavoriteList.cshtml
        
        public ActionResult Favoritelist(int? MemberID)
        {
            if (MemberID == null) { return View(); }
            ViewBag.MemberID = MemberID;
            SingleApartmentEntities db = new SingleApartmentEntities();
            List<CFavoriteList> list = new List<CFavoriteList>();
            var fav = db.FavoriteList.Where(r => r.MemberID == MemberID);
            foreach(var item in fav)
            {
                CFavoriteList Memberfav = new CFavoriteList() { entity = item };
                list.Add(Memberfav);
            }
            
            return View(list);
        }

        public ActionResult DeleteFavorite(string MemberID, string ProductID)
        {
            SingleApartmentEntities db = new SingleApartmentEntities();
            var fa = db.FavoriteList.Where(r => r.MemberID.ToString() == MemberID && r.ProductID.ToString() == ProductID).FirstOrDefault();
            if (fa != null)
            {
                db.FavoriteList.Remove(fa);
                db.SaveChanges();
            }
            return RedirectToAction("Favoritelist", new { MemberID = int.Parse(MemberID) });
        }

        #endregion

        public ActionResult PartialFavorite(string MemberID, int page = 1)
        {
            int pageSize = 2;
            SingleApartmentEntities db = new SingleApartmentEntities();
            var list = db.FavoriteList.Where(r => r.MemberID.ToString() == MemberID);
            int currentPage = page < 1 ? 1 : page;
            List<CFavoriteList> lt = new List<CFavoriteList>();
            foreach (var item in list)
            {
                lt.Add(new CFavoriteList { entity = item });
            }
            var result = lt.ToPagedList(currentPage, pageSize);
            ViewData.Model = result;
            ViewBag.MemberID = MemberID;
            return PartialView("_PartialFavorite");
        }


        #region 秉庠

        //Session
        public ActionResult Addtosession(int ID)
        {
            Product prod = (new SingleApartmentEntities().Product).Where(r => r.ProductID == ID).FirstOrDefault();

            if (prod == null) { return RedirectToAction("Index"); }

            return View(prod);
        }
        [HttpPost]
        public ActionResult Addtosession(CAddtoSessionView input)//第二步
        {
            SingleApartmentEntities db = new SingleApartmentEntities();

            Product prod = db.Product.FirstOrDefault(p => p.ProductID == input.txtProductID);

            if (prod != null)
            {
                COrderDetailsViewModel codv = new COrderDetailsViewModel();

                codv.entity = new OrderDetails();
                codv.entity.Order = new Order();
                codv.entity.ProductID = prod.ProductID;
                codv.ProductName = prod.ProductName;
                codv.ProductPrice = prod.UnitPrice;
                codv.entity.Quantity = input.txtQuantity;
                codv.entity.Order.OrderDate = DateTime.Now;


                List<COrderDetailsViewModel> list = Session[CDictionary.PRODUCTS_IN_CART] as List<COrderDetailsViewModel>;

                if (list == null)
                {
                    list = new List<COrderDetailsViewModel>();
                    Session[CDictionary.PRODUCTS_IN_CART] = list;
                }
                list.Add(codv);
            }
            return RedirectToAction("ShowProductInCart");

        }
        //刪除購物車商品(一鍵清除)11/27新增
        public ActionResult RemoveShowProductInCart(CAddtoSessionView input)
        {
            SingleApartmentEntities db = new SingleApartmentEntities();

            Product prod = db.Product.FirstOrDefault(p => p.ProductID == input.txtProductID);


            COrderDetailsViewModel codv = new COrderDetailsViewModel();


            List<COrderDetailsViewModel> list = Session[CDictionary.PRODUCTS_IN_CART] as List<COrderDetailsViewModel>;

            if (list != null)
            {
                list = new List<COrderDetailsViewModel>();
                Session[CDictionary.PRODUCTS_IN_CART] = list;
            }
            list.Remove(codv);

            return RedirectToAction("ShowProductInCart");

        }
        //刪除單一商品(11/27新增)
        public ActionResult RemoveONEShowProductInCart(int input)
        {

            SingleApartmentEntities db = new SingleApartmentEntities();

            var a = Session["txtProductID"];

            a = input;

            Product prod = db.Product.FirstOrDefault(p => p.ProductID == input);

            if (prod != null)
            {
                COrderDetailsViewModel codv = new COrderDetailsViewModel();


                List<COrderDetailsViewModel> list = Session[CDictionary.PRODUCTS_IN_CART] as List<COrderDetailsViewModel>;

                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)     //foreach沒有辦法去修改自己本身的陣列
                    {
                        if (list[i].ProductID == input)
                        {
                            list.Remove(list[i]);
                        }
                    }

                }

            }
            return RedirectToAction("ShowProductInCart");
        }


        //查看購物車
        public ActionResult ShowProductInCart()//第三步
        {
            List<COrderDetailsViewModel> list = Session[CDictionary.PRODUCTS_IN_CART] as List<COrderDetailsViewModel>;
            if (list == null)
            {
                return RedirectToAction("Home");
            }
            return View(list);
        }

        //===========================================================================
        //傳回資料庫
        [HttpPost]
        public ActionResult ShowProductInCart(int MemberID)//第四步
        {
            int totalPrice = 0;

            SingleApartmentEntities DB = new SingleApartmentEntities();

            List<COrderDetailsViewModel> list = Session[CDictionary.PRODUCTS_IN_CART] as List<COrderDetailsViewModel>;

            Order od = new Order();
            //抓出多筆資料
            foreach (var item in list)
            {
                OrderDetails ODD = new OrderDetails();

                ODD.ProductName = DB.Product.Where(p => p.ProductID == item.ProductID).FirstOrDefault().ProductName;

                ODD.Quantity = item.Quantity;

                ODD.ProductID = item.ProductID;

                ODD.UnitPrice = DB.Product.Where(p => p.ProductID == item.ProductID).FirstOrDefault().UnitPrice;

                totalPrice += item.Quantity * (DB.Product.Where(p => p.ProductID == item.ProductID).FirstOrDefault().UnitPrice);

                od.OrderDetails.Add(ODD);
            }

            //訂單日期
            od.OrderDate = DateTime.Now;
            //到貨日期
            od.ArrivedDate = DateTime.Now.AddDays(7);
            //總金額
            od.TotalAmount = totalPrice;

            od.OrderStatusID = 1;

            od.SendingStatus = "配送中";

            od.PayStatus = "尚未付款";

            od.MemberID = MemberID;//到時候要改成使用者的memberID


            DB.Order.Add(od);

            DB.SaveChanges();

            return RedirectToAction("Home");

        }
        //訂單
        public ActionResult OrderList(int order_id = 0)
        {

            bool l_flag = false;  //顯示訂單明細
            SingleApartmentEntities db = new SingleApartmentEntities();

            int member_id = db.Order.FirstOrDefault().MemberID;


            IEnumerable<Order> l_order = from x in db.Order
                                         where x.MemberID > 0   //之後要改成memberID  先抓全部
                                         select x;


            List<COrder> list = new List<COrder>();
            foreach (Order o in l_order)
            {
                list.Add(new COrder() { order_entity = o });
            }

            IEnumerable<OrderDetails> l_orderdetail = from p in db.OrderDetails
                                                      where p.OrderID == order_id
                                                      select p;
            List<COrderDetails> odlist = new List<COrderDetails>();
            foreach (OrderDetails od in l_orderdetail)
            {
                var prod = db.Product.FirstOrDefault(x => x.ProductID == od.ProductID);
                odlist.Add(new COrderDetails() { entity = od, product_entity = prod });
            }

            COrderMasterDetail a = new COrderMasterDetail() { display_flag = l_flag, t_order = list, t_orderDetail = odlist };

            return View(a);

        }
        //訂單明細
        public ActionResult List(int ID)
        {
            using (SingleApartmentEntities db = new SingleApartmentEntities())
            {
                var table = (from p in db.OrderDetails
                             where p.OrderID == ID
                             select p).ToList();

                if (table.Count == 0)
                {
                    return RedirectToAction("Home");
                }
                else
                {
                    return View(table);
                }

            }

        }
        //取消訂單
        public ActionResult Delete(int id)
        {
            SingleApartmentEntities db = new SingleApartmentEntities();

            Order od = db.Order.FirstOrDefault(p => p.OrderID == id);

            var odd = db.OrderDetails.Where(q => q.OrderID == id);

            if (odd != null)
            {

                foreach (var ITEM in odd)
                {
                    db.OrderDetails.Remove(ITEM);

                }
                if (od != null)
                {
                    db.Order.Remove(od);
                }
                db.SaveChanges();
            }

            return RedirectToAction("Home");
        }

        #endregion

    }
}