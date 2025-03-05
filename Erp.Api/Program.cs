using Erp.Infrastructure.Configurations;
using Erp.Infrastructure.Services;
using Erp.Application.Configurations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Erp.Domain.Models;
using Erp.Application.Services.UserServices;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Notifications.Configurations;
using Erp.Socket.Configurations;
using Erp.Socket.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
	options.InvalidModelStateResponseFactory = c =>
	{
		var errors = string.Join('\n', c.ModelState.Values.Where(v => v.Errors.Count > 0)
		  .SelectMany(v => v.Errors)
		  .Select(v => v.ErrorMessage));
		var response = new CustomResponseModel
		{
			IsSuccess = false,
			Message = errors,
		};
		return new BadRequestObjectResult(response);
	};
}); ;
builder.ConfigureLogging();
builder.Services.AddEndpointsApiExplorer();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerServices();
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.RegisterDatabase(builder.Configuration);
builder.Services.RegisterAutoMapper();
builder.Services.RegisterServices();
builder.Services.RegisterFluentValidation();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCaching();
builder.Services.AddCachedService<IUserService, UserService>();
builder.Services.RegisterNotificationFactory();

// Socket servislerini kaydet
builder.Services.RegisterSocketServices();

// CORS ayarları - SignalR için
builder.Services.AddCors(options =>
{
	options.AddPolicy("SignalRPolicy", 
		builder =>
		{
			builder
				.SetIsOriginAllowed(_ => true) // Tüm kaynaklara izin ver
				.AllowAnyMethod()
				.AllowAnyHeader()
				.AllowCredentials(); // SignalR için gerekli
		});
});

builder.Services
	.AddApiVersioning(option =>
	{
		option.DefaultApiVersion = new ApiVersion(1, 0);
		option.AssumeDefaultVersionWhenUnspecified = true;
		option.ApiVersionReader = new HeaderApiVersionReader();
	}); var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();

// CORS middleware'ini etkinleştir
app.UseCors("SignalRPolicy");

// Kimlik doğrulama ve yetkilendirme middleware'lerini ekle
app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerServices();
app.MapControllers();

// SignalR hub'ını yapılandır
app.MapHub<NotificationHub>("/notificationHub");

app.UseGlobalExceptionHandling();
app.UseLogUserContext();
using (var scope = app.Services.CreateScope())
{
	var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
	await seedService.SeedDataAsync();
}
app.Run();
