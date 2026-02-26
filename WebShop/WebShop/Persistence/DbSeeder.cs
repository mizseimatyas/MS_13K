using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebShop.Persistence;

namespace ModelTest
{
    public class DbSeeder
    {
        public static void Seed(DataDbContext db)
        {
            // Ha már van adat, ne seedeljünk újra
            if (db.Users.Any() || db.Categories.Any()) return;

            // Categories - csak elektronika
            var categories = new List<Category>
            {
                new Category { CategoryName = "Számítógépek" },
                new Category { CategoryName = "Kiegészítők" },
                new Category { CategoryName = "Alkatrészek" }
            };

            db.Categories.AddRange(categories);
            db.SaveChanges();

            var catComputers = categories.First(c => c.CategoryName == "Számítógépek").CategoryId;
            var catAccessories = categories.First(c => c.CategoryName == "Kiegészítők").CategoryId;
            var catParts = categories.First(c => c.CategoryName == "Alkatrészek").CategoryId;

            // Items - csak elektronikai dolgok (ID-t NEM állítunk)
            var items = new List<Item>
            {
                // Számítógépek
                new Item { CategoryId = catComputers, ItemName = "Gaming Laptop i7 RTX4070", Quantity = 8, Description = "16GB RAM, 1TB SSD", Price = 450000 },
                new Item { CategoryId = catComputers, ItemName = "MacBook Pro M3", Quantity = 5, Description = "14\"", Price = 650000 },
                new Item { CategoryId = catComputers, ItemName = "Desktop PC Ryzen7 RTX3060", Quantity = 12, Description = "32GB RAM, 1TB SSD", Price = 380000 },
                new Item { CategoryId = catComputers, ItemName = "Asus ROG Strix Laptop", Quantity = 6, Description = "i9, RTX4090, 32GB", Price = 750000 },
                new Item { CategoryId = catComputers, ItemName = "Lenovo Legion 5", Quantity = 10, Description = "Ryzen5 RTX3050", Price = 280000 },
                new Item { CategoryId = catComputers, ItemName = "Dell XPS 13", Quantity = 7, Description = "i5, 512GB SSD", Price = 320000 },

                // Kiegészítők
                new Item { CategoryId = catAccessories, ItemName = "Mechanikus Billentyűzet RGB", Quantity = 25, Description = "Cherry MX Red", Price = 35000 },
                new Item { CategoryId = catAccessories, ItemName = "Gaming Egér 16000DPI", Quantity = 30, Description = "Logitech G Pro", Price = 25000 },
                new Item { CategoryId = catAccessories, ItemName = "4K Monitor 144Hz", Quantity = 15, Description = "IPS panel", Price = 120000 },
                new Item { CategoryId = catAccessories, ItemName = "Webkamera 4K", Quantity = 20, Description = "Logitech StreamCam", Price = 45000 },
                new Item { CategoryId = catAccessories, ItemName = "Gaming Headset RGB", Quantity = 18, Description = "7.1 surround", Price = 28000 },
                new Item { CategoryId = catAccessories, ItemName = "Egérpad XXL", Quantity = 40, Description = "Speed surface", Price = 8000 },
                new Item { CategoryId = catAccessories, ItemName = "USB-C Docking Station", Quantity = 12, Description = "HDMI+DP+USB", Price = 65000 },
                new Item { CategoryId = catAccessories, ItemName = "Laptop Táska 15\"", Quantity = 22, Description = "Vízálló", Price = 15000 },
                new Item { CategoryId = catAccessories, ItemName = "RGB Ventilátor 120mm", Quantity = 50, Description = "ARGB", Price = 6000 },
                new Item { CategoryId = catAccessories, ItemName = "MSI 27\" QHD Monitor", Quantity = 9, Description = "165Hz", Price = 95000 },

                // Alkatrészek
                new Item { CategoryId = catParts, ItemName = "RTX 4080 GPU", Quantity = 6, Description = "16GB GDDR6X", Price = 320000 },
                new Item { CategoryId = catParts, ItemName = "Samsung 990 PRO 2TB SSD", Quantity = 40, Description = "PCIe 4.0", Price = 65000 },
                new Item { CategoryId = catParts, ItemName = "Corsair Vengeance 32GB RAM", Quantity = 35, Description = "DDR5 6000MHz", Price = 45000 },
                new Item { CategoryId = catParts, ItemName = "Intel i9-13900K CPU", Quantity = 4, Description = "24 magos", Price = 220000 },
                new Item { CategoryId = catParts, ItemName = "850W Gold Tápegység", Quantity = 18, Description = "Moduláris", Price = 55000 },
                new Item { CategoryId = catParts, ItemName = "Ryzen 7 7800X3D CPU", Quantity = 8, Description = "Gaming", Price = 150000 },
                new Item { CategoryId = catParts, ItemName = "RTX 4070 Ti GPU", Quantity = 10, Description = "12GB", Price = 250000 },
                new Item { CategoryId = catParts, ItemName = "WD Black SN850X 1TB SSD", Quantity = 30, Description = "PCIe 4.0", Price = 40000 },
                new Item { CategoryId = catParts, ItemName = "G.Skill Trident Z5 64GB RAM", Quantity = 5, Description = "DDR5 6400MHz", Price = 85000 },
                new Item { CategoryId = catParts, ItemName = "NZXT Kraken 360 RGB CPU hűtő", Quantity = 7, Description = "AIO", Price = 95000 },
                new Item { CategoryId = catParts, ItemName = "ASUS ROG Strix B650E", Quantity = 15, Description = "AM5 alaplap", Price = 120000 },
                new Item { CategoryId = catParts, ItemName = "Seasonic Focus GX-1000", Quantity = 12, Description = "1000W Platinum", Price = 85000 },
                new Item { CategoryId = catParts, ItemName = "Noctua NH-D15 CPU hűtő", Quantity = 20, Description = "Dual tower", Price = 35000 }
            };

            db.Items.AddRange(items);
            db.SaveChanges();

            // Users
            var users = new List<User>
            {
                new User { Email = "gamer@example.com", Password = "pass123", Address = "Budapest, PC utca 1.", Phone = 201234567, Role = "User" },
                new User { Email = "tesztfelhasznalo@example.com", Password = "pass123", Address = "Debrecen, Alkatrész u. 2.", Phone = 302345678, Role = "User" },
                new User { Email = "monitoros@example.com", Password = "pass123", Address = "Budapest, Monitor utca 7.", Phone = 303334445, Role = "User" }
            };

            db.Users.AddRange(users);
            db.SaveChanges();

            var user1Id = users[0].UserId;
            var user2Id = users[1].UserId;
            var user3Id = users[2].UserId;

            var itemLaptopId = items.First(i => i.ItemName == "Gaming Laptop i7 RTX4070").ItemId;
            var itemRtx4080Id = items.First(i => i.ItemName == "RTX 4080 GPU").ItemId;
            var itemKeyboardId = items.First(i => i.ItemName == "Mechanikus Billentyűzet RGB").ItemId;
            var itemMonitorId = items.First(i => i.ItemName == "4K Monitor 144Hz").ItemId;
            var itemRam32Id = items.First(i => i.ItemName == "Corsair Vengeance 32GB RAM").ItemId;

            // Workers
            var workers = new List<Worker>
            {
                new Worker { WorkerName = "Raktáros PC", Password = "worker123", Role = "Worker", Phone = 301112233 }
            };

            db.Workers.AddRange(workers);
            db.SaveChanges();

            // Admins
            var admins = new List<Admin>
            {
                new Admin { AdminName = "Webshop Admin", Password = "admin123", Role = "Admin" }
            };

            db.Admins.AddRange(admins);
            db.SaveChanges();

            // Carts – csak létező UserId / ItemId
            var carts = new List<Cart>
            {
                new Cart { UserId = user1Id, ItemId = itemLaptopId,  Quantity = 1, Price = 450000 },
                new Cart { UserId = user2Id, ItemId = itemRtx4080Id, Quantity = 1, Price = 320000 },
                new Cart { UserId = user1Id, ItemId = itemKeyboardId,Quantity = 2, Price = 70000 },
                new Cart { UserId = user3Id, ItemId = itemMonitorId, Quantity = 1, Price = 120000 },
                new Cart { UserId = user2Id, ItemId = itemRam32Id,   Quantity = 3, Price = 135000 }
            };

            db.Carts.AddRange(carts);
            db.SaveChanges();

            // Orders + OrderItems – szintén csak létező UserId/ItemId
            var orders = new List<Order>
            {
                new Order
                {
                    UserId = user1Id,
                    TargetAddress = "Budapest, Gaming ház 5.",
                    Date = DateTime.Now.AddDays(-3),
                    Status = OrderStatus.Delivering,
                    TotalPrice = 475000,
                    OrderItems =
                    {
                        new OrderItem { ItemId = itemLaptopId,   ItemName = "Gaming Laptop i7 RTX4070", Quantity = 1, Price = 450000 },
                        new OrderItem { ItemId = itemKeyboardId, ItemName = "Mechanikus Billentyűzet RGB", Quantity = 1, Price = 35000 }
                    }
                },
                new Order
                {
                    UserId = user2Id,
                    TargetAddress = "Debrecen, Alkatrész ház 10.",
                    Date = DateTime.Now.AddDays(-1),
                    Status = OrderStatus.PaymentSuccess,
                    TotalPrice = 65000,
                    OrderItems =
                    {
                        new OrderItem { ItemId = items.First(i => i.ItemName == "Samsung 990 PRO 2TB SSD").ItemId,
                                        ItemName = "Samsung 990 PRO 2TB SSD", Quantity = 1, Price = 65000 }
                    }
                },
                new Order
                {
                    UserId = user3Id,
                    TargetAddress = "Budapest, Monitor utca 7.",
                    Date = DateTime.Now.AddDays(-5),
                    Status = OrderStatus.OrderCompleted,
                    TotalPrice = 120000,
                    OrderItems =
                    {
                        new OrderItem { ItemId = itemMonitorId, ItemName = "4K Monitor 144Hz", Quantity = 1, Price = 120000 }
                    }
                }
            };

            db.Orders.AddRange(orders);
            db.SaveChanges();
        }
    }
}
