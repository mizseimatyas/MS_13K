using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebShop.Persistence
{
    public class DataDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }
        public DataDbContext(DbContextOptions<DataDbContext> options) : base(options) { }
    }

    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string Address { get; set; }
        public int Phone {  get; set; }
        public string Role { get; set; } = "User";
    }

    public class Worker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkerId { get; set; }
        [Required]
        public string WorkerName { get; set; }
        [Required]
        public string Password { get; set; }
        public string Role { get; set; } = "Worker";
        public int Phone { get; set; }
    }

    public class Admin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdminId { get; set; }
        [Required]
        public string AdminName { get; set; }
        [Required]
        public string Password { get; set; }
        public string Role { get; set; } = "Admin";

    }

    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<Item> Items { get; set; } = new();
    }

    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }

    }

    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public string TargetAddress { get; set; }
        public DateTimeOffset Date { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;
        public int TotalPrice { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
    }

    public class Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }

    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }

    public enum OrderStatus
    {
        Cancelled,
        DataConfirmed,
        PendingPayment,
        PaymentSuccess,
        Delivering,
        OrderCompleted
    }
}
