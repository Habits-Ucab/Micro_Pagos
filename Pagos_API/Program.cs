using Hangfire;
using Hangfire.MemoryStorage;
using MassTransit;
using MediatR;
using Pagos_Aplicacion.Commands;
using Pagos_API.Middlewares;
using Pagos_Infraestructura;
using QuestPDF.Infrastructure;
using Stripe;
using Swashbuckle.AspNetCore.Filters;
using Pagos_API.Swagger.Ejemplos;
using Pagos_Infraestructura.HangFire;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    var nombreXml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var rutaXml = Path.Combine(AppContext.BaseDirectory, nombreXml);
    if (System.IO.File.Exists(rutaXml))
    {
        opciones.IncludeXmlComments(rutaXml);
    }

    opciones.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<CrearPagoSolicitudEjemplo>();

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddMediatR(typeof(CrearPagoCommand).Assembly);

builder.Services.AgregarInfraestructura(builder.Configuration);

StripeConfiguration.ApiKey = builder.Configuration["Stripe:ClaveSecreta"];

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"];
        var usuario = builder.Configuration["RabbitMQ:Usuario"];
        var contrasena = builder.Configuration["RabbitMQ:Contrasena"];

        var hostUri = new Uri(string.IsNullOrWhiteSpace(host) ? "rabbitmq://localhost" : host);

        cfg.Host(hostUri, h =>
        {
            if (!string.IsNullOrWhiteSpace(usuario)) h.Username(usuario);
            if (!string.IsNullOrWhiteSpace(contrasena)) h.Password(contrasena);
        });
    });
});

builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseMiddleware<ManejadorExcepcionesMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard("/hangfire");
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

RecurringJob.AddOrUpdate<TrabajosProgramados>(
    "expirar_promociones",
    trabajo => trabajo.ExpirarPromocionesAsync(),
    Cron.Hourly);

RecurringJob.AddOrUpdate<TrabajosProgramados>(
    "conciliar_pagos",
    trabajo => trabajo.ConciliarPagosAsync(),
    Cron.Minutely);

app.Run();
