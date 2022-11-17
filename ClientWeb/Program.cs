using Orleans.Configuration;

var b = WebApplication.CreateBuilder(args);

b.Services
 .AddControllersWithViews()
 .AddRazorRuntimeCompilation();

b.Services.AddOrleansClient(builder =>
                            {
                                builder.UseLocalhostClustering(serviceId: "O2", clusterId: "dev")
                                       .UseConnectionRetryFilter(async (ex, cancellationToken) =>
                                                                 {
                                                                     await Task.Delay(300);
                                                                     return true;
                                                                 })
                                       .Configure<GatewayOptions>(o => { o.GatewayListRefreshPeriod = TimeSpan.FromSeconds(5); })
                                       .Configure<ClientMessagingOptions>(o => { o.ResponseTimeout  = TimeSpan.FromSeconds(30); });
                            });

// b.Services.AddSingleton<ClusterClientHostedService>();
// b.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<ClusterClientHostedService>());
// b.Services.AddSingleton<IClusterClient>(sp => sp.GetRequiredService<ClusterClientHostedService>().client);
// b.Services.AddSingleton<IGrainFactory>(sp => sp.GetRequiredService<ClusterClientHostedService>().client);

var app = b.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(name: "default",
                       pattern: "{controller=Home}/{action=Index}/{id?}");

// app.Services.GetRequiredService<IClusterClient>().
app.Run();