using Microsoft.EntityFrameworkCore;
using WebShop.Persistence;

namespace WebShop.Persistence
{
    public static class DbSeeder
    {
        public static void Seed(DataDbContext db)
        {
            // Ha már van adat, ne seedeljünk újra
            if (db.Users.Any() || db.Categories.Any()) return;

            // Categories - csak elektronika
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "Számítógépek" },
                new Category { CategoryId = 2, CategoryName = "Kiegészítők" },
                new Category { CategoryId = 3, CategoryName = "Alkatrészek" }
            };

            db.Categories.AddRange(categories);
            db.SaveChanges();

            // Items - csak elektronikai dolgok
            var items = new List<Item>
            {
                // Számítógépek
                new Item { ItemId = 1, CategoryId = 1, ItemName = "Gaming Laptop i7 RTX4070", Quantity = 8, Description = "16GB RAM, 1TB SSD", Price = 450000 },
                new Item { ItemId = 2, CategoryId = 1, ItemName = "MacBook Pro M3", Quantity = 5, Description = "14\"", Price = 650000 },
                new Item { ItemId = 3, CategoryId = 1, ItemName = "Desktop PC Ryzen7 RTX3060", Quantity = 12, Description = "32GB RAM, 1TB SSD", Price = 380000 },
                new Item { ItemId = 13, CategoryId = 1, ItemName = "Asus ROG Strix Laptop", Quantity = 6, Description = "i9, RTX4090, 32GB", Price = 750000 },
                new Item { ItemId = 14, CategoryId = 1, ItemName = "Lenovo Legion 5", Quantity = 10, Description = "Ryzen5 RTX3050", Price = 280000 },
                new Item { ItemId = 15, CategoryId = 1, ItemName = "Dell XPS 13", Quantity = 7, Description = "i5, 512GB SSD", Price = 320000 },
                
                // Kiegészítők
                new Item { ItemId = 4, CategoryId = 2, ItemName = "Mechanikus Billentyűzet RGB", Quantity = 25, Description = "Cherry MX Red", Price = 35000 },
                new Item { ItemId = 5, CategoryId = 2, ItemName = "Gaming Egér 16000DPI", Quantity = 30, Description = "Logitech G Pro", Price = 25000 },
                new Item { ItemId = 6, CategoryId = 2, ItemName = "4K Monitor 144Hz", Quantity = 15, Description = "IPS panel", Price = 120000 },
                new Item { ItemId = 7, CategoryId = 2, ItemName = "Webkamera 4K", Quantity = 20, Description = "Logitech StreamCam", Price = 45000 },
                new Item { ItemId = 16, CategoryId = 2, ItemName = "Gaming Headset RGB", Quantity = 18, Description = "7.1 surround", Price = 28000 },
                new Item { ItemId = 17, CategoryId = 2, ItemName = "Egérpad XXL", Quantity = 40, Description = "Speed surface", Price = 8000 },
                new Item { ItemId = 18, CategoryId = 2, ItemName = "USB-C Docking Station", Quantity = 12, Description = "HDMI+DP+USB", Price = 65000 },
                new Item { ItemId = 19, CategoryId = 2, ItemName = "Laptop Táska 15\"", Quantity = 22, Description = "Vízálló", Price = 15000 },
                new Item { ItemId = 20, CategoryId = 2, ItemName = "RGB Ventilátor 120mm", Quantity = 50, Description = "ARGB", Price = 6000 },
                new Item { ItemId = 21, CategoryId = 2, ItemName = "MSI 27\" QHD Monitor", Quantity = 9, Description = "165Hz", Price = 95000 },
                
                // Alkatrészek
                new Item { ItemId = 8, CategoryId = 3, ItemName = "RTX 4080 GPU", Quantity = 6, Description = "16GB GDDR6X", Price = 320000 },
                new Item { ItemId = 9, CategoryId = 3, ItemName = "Samsung 990 PRO 2TB SSD", Quantity = 40, Description = "PCIe 4.0", Price = 65000 },
                new Item { ItemId = 10, CategoryId = 3, ItemName = "Corsair Vengeance 32GB RAM", Quantity = 35, Description = "DDR5 6000MHz", Price = 45000 },
                new Item { ItemId = 11, CategoryId = 3, ItemName = "Intel i9-13900K CPU", Quantity = 4, Description = "24 magos", Price = 220000 },
                new Item { ItemId = 12, CategoryId = 3, ItemName = "850W Gold Tápegység", Quantity = 18, Description = "Moduláris", Price = 55000 },
                new Item { ItemId = 22, CategoryId = 3, ItemName = "Ryzen 7 7800X3D CPU", Quantity = 8, Description = "Gaming", Price = 150000 },
                new Item { ItemId = 23, CategoryId = 3, ItemName = "RTX 4070 Ti GPU", Quantity = 10, Description = "12GB", Price = 250000 },
                new Item { ItemId = 24, CategoryId = 3, ItemName = "WD Black SN850X 1TB SSD", Quantity = 30, Description = "PCIe 4.0", Price = 40000 },
                new Item { ItemId = 25, CategoryId = 3, ItemName = "G.Skill Trident Z5 64GB RAM", Quantity = 5, Description = "DDR5 6400MHz", Price = 85000 },
                new Item { ItemId = 26, CategoryId = 3, ItemName = "NZXT Kraken 360 RGB CPU hűtő", Quantity = 7, Description = "AIO", Price = 95000 },
                new Item { ItemId = 27, CategoryId = 3, ItemName = "ASUS ROG Strix B650E", Quantity = 15, Description = "AM5 alaplap", Price = 120000 },
                new Item { ItemId = 28, CategoryId = 3, ItemName = "Seasonic Focus GX-1000", Quantity = 12, Description = "1000W Platinum", Price = 85000 },
                new Item { ItemId = 29, CategoryId = 3, ItemName = "Noctua NH-D15 CPU hűtő", Quantity = 20, Description = "Dual tower", Price = 35000 }
            };

            db.Items.AddRange(items);
            db.SaveChanges();

            // Users
            var users = new List<User>
            {
                new User { UserId = 1, Email = "gamer@example.com", Password = "pass123", Address = "Budapest, PC utca 1.", Phone = 201234567, Role = "User" },
                new User { UserId = 2, Email = "tesztfelhasznalo@example.com", Password = "pass123", Address = "Debrecen, Alkatrész u. 2.", Phone = 302345678, Role = "User" }
            };

            db.Users.AddRange(users);
            db.SaveChanges();

            // Workers
            var workers = new List<Worker>
            {
                new Worker { WorkerId = 1, WorkerName = "Raktáros PC", Password = "worker123", Role = "Worker", Phone = 301112233 }
            };

            db.Workers.AddRange(workers);
            db.SaveChanges();

            // Admins
            var admins = new List<Admin>
            {
                new Admin { AdminId = 1, AdminName = "Webshop Admin", Password = "admin123", Role = "Admin" }
            };

            db.Admins.AddRange(admins);
            db.SaveChanges();

            // Carts
            var carts = new List<Cart>
            {
               new Cart { CartId = 1, UserId = 1, ItemId = 1, Quantity = 1, Price = 450000 },
                new Cart { CartId = 2, UserId = 2, ItemId = 8, Quantity = 1, Price = 320000 },
                new Cart { CartId = 3, UserId = 1, ItemId = 4, Quantity = 2, Price = 70000 },
                new Cart { CartId = 4, UserId = 3, ItemId = 6, Quantity = 1, Price = 120000 },
                new Cart { CartId = 5, UserId = 2, ItemId = 10, Quantity = 3, Price = 135000 }
            };

            db.Carts.AddRange(carts);
            db.SaveChanges();

            // Orders
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = 1,
                    UserId = 1,
                    TargetAddress = "Budapest, Gaming ház 5.",
                    Date = DateTimeOffset.Now.AddDays(-3),
                    Status = OrderStatus.Delivering,
                    TotalPrice = 475000,
                    OrderItems =
                    {
                        new OrderItem { OrderItemId = 1, ItemId = 1, ItemName = "Gaming Laptop i7 RTX4070", Quantity = 1, Price = 450000 },
                        new OrderItem { OrderItemId = 2, ItemId = 4, ItemName = "Mechanikus Billentyűzet RGB", Quantity = 1, Price = 35000 }
                    }
                },
                new Order
                {
                    OrderId = 2,
                    UserId = 2,
                    TargetAddress = "Debrecen, Alkatrész ház 10.",
                    Date = DateTimeOffset.Now.AddDays(-1),
                    Status = OrderStatus.PaymentSuccess,
                    TotalPrice = 65000,
                    OrderItems = { new OrderItem { OrderItemId = 3, ItemId = 9, ItemName = "Samsung 990 PRO 2TB SSD", Quantity = 1, Price = 65000 } }
                },
                new Order
                {
                    OrderId = 3, UserId = 3, TargetAddress = "Budapest, Monitor utca 7.", Date = DateTimeOffset.Now.AddDays(-5),
                    Status = OrderStatus.OrderCompleted, TotalPrice = 120000,
                    OrderItems = { new OrderItem { OrderItemId = 4, ItemId = 6, ItemName = "4K Monitor 144Hz", Quantity = 1, Price = 120000 } }
                }
            };
            db.Orders.AddRange(orders);
            db.SaveChanges();
        }
    }
}
