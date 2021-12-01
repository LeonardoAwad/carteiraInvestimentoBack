using CalculoInvestimento.Back.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>();

string corsPolicy = "_corsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy,
        builder => builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});
var key = Encoding.ASCII.GetBytes(Settings.Secret);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();

//Routes
app.MapGet("/", (Func<string>)(() => "Hello World!"));

app.MapGet("/users", async (http) =>
{
    var context = http.RequestServices.GetService<AppDbContext>();
    var users = await context.Users.ToListAsync();

    await http.Response.WriteAsJsonAsync(users);
}).RequireAuthorization();

app.MapGet("/users/{id}", async (http) =>
{
    if (!http.Request.RouteValues.TryGetValue("id", out var id))
    {
        http.Response.StatusCode = 400;
    }

    var dbContext = http.RequestServices.GetService<AppDbContext>();
    var user = await dbContext.Users.FindAsync(int.Parse(id?.ToString() ?? "0"));
    if (user == null)
    {
        http.Response.StatusCode = 400;
    }
    
    await http.Response.WriteAsJsonAsync(user);
}).RequireAuthorization();

app.MapPost("/user", async (http) =>
{
    var user = await http.Request.ReadFromJsonAsync<User>();
    var dbContext = http.RequestServices.GetService<AppDbContext>();
    
    await dbContext.Users.AddAsync(user);
    await dbContext.SaveChangesAsync();
    
    http.Response.StatusCode = 201;
}).AllowAnonymous();

app.MapPost("/investments", async (http) =>
{
    http.Request.Headers.TryGetValue("UserId", out var userId);
    var investiments = await http.Request.ReadFromJsonAsync<Investiments>();
    var dbContext = http.RequestServices.GetService<AppDbContext>();

    investiments.User = dbContext.Users.Find(Guid.Parse(userId.ToString()));

    if(investiments.User == null)
    {
        http.Response.StatusCode = 404;
        http.Response.WriteAsJsonAsync("User not found!");
    }

    await dbContext.Investiments.AddAsync(investiments);
    await dbContext.SaveChangesAsync();

    http.Response.StatusCode = 201;
}).RequireAuthorization();

app.MapGet("/investments", async (http) =>
{
    var context = http.RequestServices.GetService<AppDbContext>();
    http.Request.Headers.TryGetValue("UserId", out var userId);
    var investiments = await context.Investiments
    .Where(x => x.User.Id == Guid.Parse(userId.ToString()))
    .ToListAsync();

    await http.Response.WriteAsJsonAsync(investiments);
}).RequireAuthorization();

app.MapDelete("/investments/{id}", async (http) =>
{
    if (!http.Request.RouteValues.TryGetValue("id", out var id))
    {
        http.Response.StatusCode = 400;
    }

    var dbContext = http.RequestServices.GetService<AppDbContext>();
    var investiments = await dbContext.Investiments.FindAsync(Guid.Parse(id?.ToString() ?? "0"));
   
    if (investiments == null)
    {
        http.Response.StatusCode = 404;
    }

    var investimentsDelete = dbContext.Investiments.Remove(investiments);
    dbContext.SaveChanges();

    http.Response.StatusCode = 204;
}).RequireAuthorization();

//AuthRoutes
app.MapPost("/login", async (http) =>
{
    var loginData = await http.Request.ReadFromJsonAsync<User>();

    var dbContext = http.RequestServices.GetService<AppDbContext>();
    var user = await dbContext.Users
    .Where(x => x.Username == loginData.Username && x.Password == loginData.Password)
    .FirstOrDefaultAsync();
    if(user == null)
    {
        http.Response.StatusCode = 404;
    }

    var token = TokenService.GenerateToken(user);

    user.Password = "";

    http.Response.StatusCode = 200;
    await http.Response.WriteAsJsonAsync(new { user = user, token = token }); 
}).AllowAnonymous();

app.Run();