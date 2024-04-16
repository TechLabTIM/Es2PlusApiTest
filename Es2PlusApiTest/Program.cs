using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

var certificatePath = builder.Configuration.GetValue<string>("CertificateSettings:CertificatePath")
    ?? Environment.GetEnvironmentVariable("CERTIFICATE_PATH");
var certificatePassword = builder.Configuration.GetValue<string>("CertificateSettings:CertificatePassword")
    ?? Environment.GetEnvironmentVariable("CERTIFICATE_PASSWORD");

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        if (!string.IsNullOrEmpty(certificatePath) && !string.IsNullOrEmpty(certificatePassword))
        {
            httpsOptions.ServerCertificate = new X509Certificate2(certificatePath, certificatePassword);
        }
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
//app.MapControllers();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
//app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


app.Run();

