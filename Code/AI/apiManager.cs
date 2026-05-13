using Sandbox;
using System.Collections.Generic;

public class ApiProvider
{
    public string Name { get; set; }
    public string Model { get; set; }
    public string Key { get; set; }
    public string Url { get; set; } = "https://openrouter.ai/api/v1/chat/completions";
}

public sealed class ApiManager : Component
{
    // Mantenemos la propiedad por si quieres verlos en el inspector, 
    // pero el código los sobreescribirá al empezar.
    [Property] public List<ApiProvider> Providers { get; set; } = new();
    private int _currentIndex = 0;

    protected override void OnStart()
    {
        // Forzamos el hardcodeo limpiando cualquier dato previo
        Providers.Clear();

		// API 1: Gemma (rapida y eficiente)
        Providers.Add(new ApiProvider {
            Name = "Fish1_Gemma",
            Model = "google/gemma-4-26b-a4b-it:free",
            Key = "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"
        });

		// API 2: Nemotron (otra de gemini, para reintentar la petición)
        Providers.Add(new ApiProvider {
            Name = "Fish2_Gemma",
            Model = "google/gemma-4-31b-it:free",
            Key = "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"
        });

        // API 3: Owl-Alpha (Respaldo)
        Providers.Add(new ApiProvider {
            Name = "Fish3_Owl",
            Model = "openrouter/owl-alpha",
            Key = "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"
        });

		// API 4: Nemotron (Respaldo)
        Providers.Add(new ApiProvider {
            Name = "Fish4_Nemotron",
            Model = "nvidia/nemotron-3-nano-30b-a3b:free",
            Key = "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"
        });


        Log.Info($"[API] Manager iniciado con {Providers.Count} proveedores configurados.");
    }

    public ApiProvider GetActiveProvider()
    {
        if ( Providers == null || Providers.Count == 0 ) return null;
        return Providers[_currentIndex % Providers.Count];
    }

    public void SwitchToNextProvider()
    {
        if ( Providers.Count <= 1 ) return;
        _currentIndex = (_currentIndex + 1) % Providers.Count;
        Log.Warning( $"⚠️ [API] Cambio automático a respaldo: {GetActiveProvider().Name}" );
    }
}
