using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> InitiatePayment()
        {
            string apiKey = "ZXlKaGJHY2lPaUpJVXpVeE1pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SmpiR0Z6Y3lJNklrMWxjbU5vWVc1MElpd2ljSEp2Wm1sc1pWOXdheUk2TVRBek1UVTVNU3dpYm1GdFpTSTZJakUzTkRJd056VXpNelV1TlRnd05qTXhJbjAubXpHVzF1azJ5NE8wQ1JHWXlpOGp4THpKaFFKR0tQdERzdnhPc0x4bWN2Ulp3TVNUU3BiUGJFVHlRMGxlMnh3ZFQ4Q0xRUzhHaWxfZ2pGdkNCS283bmc="; // استبدليه بالمفتاح بتاعك

            var authRequest = new
            {
                api_key = apiKey
            };

            using var client = new HttpClient();
            var authResponse = await client.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", authRequest);
            var authData = await authResponse.Content.ReadFromJsonAsync<dynamic>();

            string token = authData.token;

            var orderRequest = new
            {
                auth_token = token,
                delivery_needed = "false",
                amount_cents = "10000", 
                currency = "EGP",
                items = new object[] { }
            };

            var orderResponse = await client.PostAsJsonAsync("https://accept.paymob.com/api/ecommerce/orders", orderRequest);
            var orderData = await orderResponse.Content.ReadFromJsonAsync<dynamic>();

            int orderId = orderData.id;

            var paymentKeyRequest = new
            {
                auth_token = token,
                amount_cents = "10000",
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    first_name = "Menna",
                    last_name = "User",
                    email = "menna@example.com",
                    phone_number = "01000000000",
                    city = "Cairo",
                    country = "EG",
                    street = "Test Street",
                    building = "10",
                    apartment = "5",
                    floor = "3"
                },
                currency = "EGP",
                integration_id = "5017851"
            };

            var paymentKeyResponse = await client.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", paymentKeyRequest);
            var paymentKeyData = await paymentKeyResponse.Content.ReadFromJsonAsync<dynamic>();

            string paymentKey = paymentKeyData.token;

            return Redirect($"https://accept.paymob.com/api/acceptance/iframes/908226?payment_token={paymentKey}");
        }

        [HttpPost]
        public IActionResult Callback([FromBody] dynamic response)
        {
            if (response.success == true)
            {
                return View("Success");
            }
            else
            {
                return View("Failed"); 
            }
        }


    }
}
