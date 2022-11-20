using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Orleans.Configuration;
using Orleans.Runtime;

var b = WebApplication.CreateBuilder(args);

b.Services
 .AddControllersWithViews()
 .AddRazorRuntimeCompilation();
b.Services.AddAuthentication(o =>
                             {
                                 o.DefaultScheme              = CookieAuthenticationDefaults.AuthenticationScheme;
                                 o.RequireAuthenticatedSignIn = false;
                             })
 .AddCookie(o =>
            {
                o.Cookie = new CookieBuilder()
                           {
                               Name         = "O2-Auth",
                               IsEssential  = true,
                               SecurePolicy = CookieSecurePolicy.None,
                               HttpOnly     = true
                           };
                o.ExpireTimeSpan    = TimeSpan.FromDays(31);
                o.SlidingExpiration = true;
            });
b.Services.AddHttpContextAccessor();
b.Services.AddAuthorization();

b.Services.AddOrleansClient(c =>
                            {
                                c.AddOutgoingGrainCallFilter(async context =>
                                                             {
                                                                 RequestContext.Set("intercepted value", "this value was added by the filter");
                                                                 await context.Invoke();
                                                             });

                                c.Configure<ClusterOptions>(o =>
                                                            {
                                                                o.ClusterId = "dev";
                                                                o.ServiceId = "O2";
                                                            });
                                c.UseAdoNetClustering(o =>
                                                      {
                                                          o.Invariant = "System.Data.SqlClient";
                                                          o.ConnectionString = @"Data Source=localhost,1433;"                      +
                                                                               "Initial Catalog=Orleans;Integrated Security=True;" +
                                                                               "Pooling=False;Max Pool Size=200";
                                                      })
                                 .UseConnectionRetryFilter(async (ex, cancellationToken) =>
                                                           {
                                                               Console.WriteLine(ex);
                                                               await Task.Delay(300);
                                                               return true;
                                                           })
                                 .Configure<GatewayOptions>(o => { o.GatewayListRefreshPeriod = TimeSpan.FromSeconds(10); })
                                 .Configure<ClientMessagingOptions>(o => { o.ResponseTimeout  = TimeSpan.FromSeconds(1); });
                            });

var app = b.Build();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();