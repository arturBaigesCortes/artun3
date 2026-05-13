using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Linq;

// --- API Utility Classes ---
public class ChatEntry { public string role { get; set; } public string content { get; set; } }
public class ChatApiResponse { public List<ChatChoice> choices { get; set; } }
public class ChatChoice { public ChatEntry message { get; set; } }

public sealed class FishBrain : Component
{
    [Property] public FishResource Data { get; set; }
    
    private List<ChatEntry> Memoria = new List<ChatEntry>();
    private bool _isThinking = false; 

	public bool HasTalkedTo { get; set; } = false;

    protected override void OnStart()
    {
        if ( Data != null ) CargarPez( Data );
    }



    /// Iinicileza los recursos .fish que declaramos
    public void CargarPez( FishResource nuevoPez )
    {
        if ( nuevoPez == null ) return;
        Data = nuevoPez;
        Memoria.Clear();
        HasTalkedTo = false;
        
        // se envia el comportamiento del pez a la ia
        Memoria.Add( new ChatEntry { 
            role = "system", 
            content = $"{Data.Personality} Responde siempre en español. Sé breve y creativo. No uses emojis." 
        } );
    }

    
    /// Se le envia lo que ha dicho el usuario
    public async Task<string> HablarConIA( string mensajeUsuario )
    {
        if ( _isThinking ) return "..."; // Busy signal

        var apiManager = Scene.GetAllComponents<ApiManager>().FirstOrDefault();
        if ( apiManager == null ) return "Glub... (Falta el ApiManager en la escena)";

        _isThinking = true;
        Memoria.Add( new ChatEntry { role = "user", content = mensajeUsuario } );
        
        // memoria de 8 mensajes
        if ( Memoria.Count > 8 ) Memoria.RemoveAt( 1 );

        // Reintenta el prompt con diferentes ias
        for ( int attempt = 0; attempt < apiManager.Providers.Count; attempt++ )
        {
            var provider = apiManager.GetActiveProvider();
            Log.Info( $"[AI] Attempt {attempt + 1}: Querying {provider.Name}..." );

            
            using var cts = new CancellationTokenSource( TimeSpan.FromSeconds( 8 ) );

            try
            {
                var payload = new {
                    model = provider.Model,
                    messages = Memoria,
                    temperature = Data?.Temperature ?? 1.0f,
                    
                };

                var json = JsonSerializer.Serialize( payload );
                var content = new StringContent( json, Encoding.UTF8, "application/json" );

                var headers = new Dictionary<string, string> { 
                    { "Authorization", $"Bearer {provider.Key}" },
                    { "HTTP-Referer", "https://sbox.game" },
                    { "X-Title", "Sbox Fishing" }
                };

                var response = await Http.RequestAsync( "https://openrouter.ai/api/v1/chat/completions", "POST", content, headers );

                if ( response.StatusCode == System.Net.HttpStatusCode.OK )
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ChatApiResponse>( responseBody );
                    
                    string reply = result?.choices?.FirstOrDefault()?.message?.content;

                    // comprueba si la IA ha respondido algo
                    if ( !string.IsNullOrWhiteSpace( reply ) )
                    {
                        Memoria.Add( new ChatEntry { role = "assistant", content = reply } );
                        _isThinking = false;
						Scene.GetAllComponents<Artun2.TutorialController>().FirstOrDefault()?.OnTalkedToFish();
                        return reply;
						
                    }
                    
                    Log.Warning( $"[AI] {provider.Name} returned an empty response. Rotating..." );
                }
                else
                {
                    Log.Warning( $"[AI] {provider.Name} error: {response.StatusCode}" );
                }
            }
            catch ( Exception e )
            {
                Log.Error( $"[AI] Connection error on {provider.Name}: {e.Message}" );
            }

            // Cambia de ia en caso no exitoso de respuesta
            apiManager.SwitchToNextProvider();
        }

        _isThinking = false;
        return "Glub... (No puedo pensar ahora mismo, inténtalo de nuevo)";
    }
}
