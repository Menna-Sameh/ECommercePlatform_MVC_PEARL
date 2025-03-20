using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DataAcessLayer.Models;

namespace PresentationLayer.Views.Shared.Components.CartSummary
{
    public class CartSummaryViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = GetCart();
            int itemCount = cart.Sum(item => item.Quantity); // إجمالي عدد المنتجات في السلة
            return View(itemCount);
        }

        private List<CartItem> GetCart()
        {
            var cartSession = HttpContext.Session.GetString("ShoppingCart");
            return cartSession != null ? JsonConvert.DeserializeObject<List<CartItem>>(cartSession) : new List<CartItem>();
        }
    }
}
