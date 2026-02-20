using MiniERP.Persistence;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddPersistenceServices(builder.Configuration);

builder.Services.AddControllers();
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
