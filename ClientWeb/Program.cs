using ClientWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;
using Serilog;
using Shared;

var b = WebApplication.CreateBuilder(args);
b.Configuration.ConfigureAppSettings();
b.Host.UseSerilog((context, services, config) => config.ReadFrom.Configuration(context.Configuration)
                                                       .ReadFrom.Services(services)
                                                       .Enrich.FromLogContext());
b.Services
 .AddControllersWithViews()
 .AddRazorRuntimeCompilation();

b.Services
 .AddAuthentication(o =>
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
                               HttpOnly     = false
                           };
                o.ExpireTimeSpan    = TimeSpan.FromDays(31);
                o.SlidingExpiration = true;
            });
b.Services.AddHttpContextAccessor();
b.Services.AddAuthorization();
b.Services.Configure<Config>(b.Configuration.GetSection("Def"));

b.Services.AddOrleansClient(c =>
                            {
                                c.AddOutgoingGrainCallFilter(async ctx =>
                                                             {
                                                                 RequestContext.Set("intercepted value", "this value was added by the filter");
                                                                 await ctx.Invoke();
                                                             });

                                c.Configure<ClusterOptions>(o =>
                                                            {
                                                                o.ClusterId = OrleansConfig.ClusterId;
                                                                o.ServiceId = OrleansConfig.ServiceId;
                                                            });

                                c.ConfigureServices(s => s.AddSingleton<IClientConnectionRetryFilter, ConnectionRetryFilter>());
                                // c.UseMongoDBClient(OrleansConfig.MongoUrl)
                                //  .UseMongoDBClustering(o =>
                                //                        {
                                //                            o.DatabaseName = OrleansConfig.DatabaseName;
                                //                            o.Strategy     = MongoDBMembershipStrategy.SingleDocument;
                                //                        });
                                c.Configure<GatewayOptions>(o => o.GatewayListRefreshPeriod = TimeSpan.FromSeconds(10))
                                 .Configure<ClientMessagingOptions>(o => o.ResponseTimeout  = TimeSpan.FromSeconds(1));
                            });

var app = b.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

var s = app.Services.GetRequiredService<IOptionsMonitor<Config>>();
s.OnChange(c => Console.WriteLine("Config changed"));

app.Run();