namespace DigitalSignage.Api.Services.Weather;

public static class WeatherCodeMapper
{
    public static string ToDescription(int code) =>
        code switch
        {
            0 => "Muy soleado",
            1 => "Soleado",
            2 => "Parcialmente nublado",
            3 => "Nublado",
            45 or 48 => "Niebla",
            51 or 53 or 55 => "Llovizna",
            56 or 57 => "Llovizna helada",
            61 or 63 or 65 => "Lluvia",
            66 or 67 => "Lluvia helada",
            71 or 73 or 75 or 77 => "Nieve",
            80 or 81 or 82 => "Chubascos",
            85 or 86 => "Chubascos de nieve",
            95 => "Tormenta",
            96 or 99 => "Tormenta con granizo",
            _ => "Condición no disponible"
        };
}
