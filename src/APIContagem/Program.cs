using APIContagem;
using APIContagem.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Contador>();

builder.Services.AddOpenApi();
builder.Services.AddCors();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "API de Contagem de Acessos";
    options.Theme = ScalarTheme.BluePlanet;
    options.DarkMode = true;
});

app.UseRouting();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseHttpsRedirection();

Lock ContagemLock = new();
app.MapGet("/contador", (Contador contador) =>
{
    int valorAtualContador;
    using (ContagemLock.EnterScope())
    {
        contador.Incrementar();
        valorAtualContador = contador.ValorAtual;
    }
    app.Logger.LogInformation($"Contador - Valor atual: {valorAtualContador}");
    if (contador.ValorAtual % 2 == 0)
    {
        return Results.Ok(new ResultadoContadorUnion(valorAtualContador));
    }
    else
    {
        var resultadoContador = new ResultadoContador()
        {
            ValorAtual = contador.ValorAtual,
            Local = contador.Local,
            Kernel = contador.Kernel,
            Framework = contador.Framework,
            Mensagem = app.Configuration["Saudacao"]
        };
        return Results.Ok(new ResultadoContadorUnion(resultadoContador));
    }
})
.Produces<ResultadoContadorUnion>();

app.Run();