using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using WeCarry.Models;
using WeCarry.Models.MVVM;
using WeCarry.Services;

var builder = WebApplication.CreateBuilder(args);

// --- DATABASE ---
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- CONTROLLERS + VIEWS ---
builder.Services.AddControllersWithViews();

// --- DEPENDENCY INJECTION ---
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IAdsRepository, AdsRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// --- SIGNALR ---
builder.Services.AddSignalR();

// --- SESSION + CACHE ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // Oturum süresi
    options.Cookie.HttpOnly = true;                 // Tarayýcý tarafýndan eriþilemez
    options.Cookie.IsEssential = true;              // Çerez politikalarýndan etkilenmez
});

// --- HTTP CONTEXT ACCESSOR ---
builder.Services.AddHttpContextAccessor();

// --- RESPONSE COMPRESSION (gzip + brotli) ---
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;                      // HTTPS'te de sýkýþtýrma aktif
    options.Providers.Add<GzipCompressionProvider>();   // Gzip formatý
    options.Providers.Add<BrotliCompressionProvider>(); // Brotli formatý
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

var app = builder.Build();

// --- GLOBAL EXCEPTION HANDLING ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// --- PIPELINE ---
// HTTPS yönlendirmesi (http -> https)
app.UseHttpsRedirection();

// 1?? Statik dosyalardan ÖNCE compression aktif edilmeli
app.UseResponseCompression();

// 2?? Statik dosyalar (cache ile)
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // 7 gün cache tut
        const int durationInSeconds = 60 * 60 * 24 * 7;
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=" + durationInSeconds;
    }
});

app.UseRouting();

// 3?? Session
app.UseSession();

// 4?? Authentication + Authorization
// app.UseAuthentication();
app.UseAuthorization();

// --- ROUTES ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- SIGNALR HUB ---
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
