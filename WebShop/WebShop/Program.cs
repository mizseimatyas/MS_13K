using Microsoft.EntityFrameworkCore;
using WebShop.Model;
using WebShop.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContextPool<DataDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Connect")));
builder.Services.AddTransient<ItemModel>();
builder.Services.AddTransient<CategoryModel>();
builder.Services.AddTransient<CartModel>();
builder.Services.AddTransient<OrderModel>();
builder.Services.AddTransient<WorkerModel>();
builder.Services.AddTransient<AdminModel>();
builder.Services.AddTransient<UserModel>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
