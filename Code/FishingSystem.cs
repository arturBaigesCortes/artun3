using Sandbox;
using System.Linq;
using System.Collections.Generic; // Necesario para usar List

public sealed class FishingSystem : Component
{
    [Property] public Inventory PlayerInventory { get; set; }
    
    // Lista de peces que pueden salir. Arrastra aquí todos tus archivos .fish
    [Property] public List<FishResource> PosiblesCapturas { get; set; } = new();

    [Property] public SceneFile MyMap { get; set; }

    protected override void OnStart()
    {
        if ( MyMap == null )
        {
            Log.Warning( "Map property is empty! Drag 'Water Train Station' into the Inspector slot." );
        }
    }

    public void CaptureFish()
    {
        // 1. Verificamos si hay peces en la lista para evitar errores
        if ( PosiblesCapturas == null || PosiblesCapturas.Count == 0 )
        {
            Log.Error( "¡La lista de PosiblesCapturas está vacía! Añade peces en el Inspector." );
            return;
        }

        // 2. Buscamos el inventario si no lo tenemos asignado
        if ( PlayerInventory == null )
        {
            PlayerInventory = Scene.GetAllComponents<Inventory>().FirstOrDefault();
        }

        if ( PlayerInventory != null )
        {
            if ( PlayerInventory.CaughtFishes.Count < 16 )
            {
                // 3. SELECCIÓN ALEATORIA: Elegimos uno al azar de la lista
                var pezAleatorio = Game.Random.FromList( PosiblesCapturas );

                if ( pezAleatorio != null )
                {
                    // Añadimos el pez específico que salió en el "sorteo"
                    PlayerInventory.CaughtFishes.Add( pezAleatorio );
                    Log.Info( $"¡Has capturado un {pezAleatorio.Name}!" );
                }
            }
            else
            {
                Log.Warning( "Inventario lleno (Debug)" );
            }
        }
        else
        {
            Log.Error( "No se encontró el componente Inventory en la escena." );
        }
    }

    protected override void OnUpdate()
    {
        // Simular pesca con Espacio
        if ( Input.Pressed( "Debug3" ) )
        {
            CaptureFish();
        }
    }
}
