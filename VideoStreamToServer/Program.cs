using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using VideoStreamToServer.Configurations;
using VideoStreamToServer.Extensions;
var builder = WebApplication.CreateBuilder(args);

var UploadSettings = builder.Configuration.GetSection("FileUpload").Get<FileUploadSettings>();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(name: MyAllowSpecificOrigins,
//                      builder =>
//                      {
//                          builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();

//                      });
//});




// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<FormOptions>(options =>
{
    // Set the limit to 256 MB
    options.MultipartBodyLengthLimit = 2684354560;
});
builder.Services.AddSingleton(UploadSettings);
var app = builder.Build();
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
     .WithExposedHeaders("*")
     );

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.ConfigureTusChunkUpload("/files", UploadSettings.UploadPath);
 
  

app.Run();
