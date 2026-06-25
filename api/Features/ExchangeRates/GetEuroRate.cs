using Api.Infrastructure;

namespace Api.Features.ExchangeRates;

public record EuroRateResponse(decimal Rate, string Code);

public static class GetEuroRate
{
    public static IEndpointRouteBuilder MapGetEuroRateEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rates/euro", Handle)
            .WithName("GetEuroRate")
            .WithTags("ExchangeRates");
        return app;
    }

    private static async Task<IResult> Handle(NbpClient nbpClient, CancellationToken ct)
    {
        var rate = await nbpClient.GetEuroRateAsync(ct);

        return rate is null
            ? Results.Problem("Failed to fetch euro rate from NBP", statusCode: 502)
            : Results.Ok(new EuroRateResponse(rate.Value, "EUR"));
    }
}
