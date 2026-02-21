using MiniERP.Application;
using MiniERP.Persistence;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Eğer bir değer null ise, onu JSON sonucuna dahil etme (gizle)
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    }); 


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Sıralama önemli. Önce kimlik doğrulanır (Kimsin?), sonra yetki kontrol edilir (Neye yetkin var?).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
