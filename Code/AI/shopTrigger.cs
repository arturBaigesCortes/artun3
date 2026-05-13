using Sandbox;
using System.Linq;

public sealed class ShopTrigger : Component
{
    [Property] public float InteractRadius { get; set; } = 150f;
    [Property] public GameObject PlayerObject { get; set; }

    private bool _playerNearby = false;

	//si el usuaro se acerca en un area a la tienda, esta envia una señal para abrir la info de de la tienda, con todos los componentes a comprar
    protected override void OnUpdate()
    {
        if (PlayerObject == null) return;

        float dist = Vector3.DistanceBetween(
            Transform.Position, 
            PlayerObject.Transform.Position
        );

        bool isCurrentlyNearby = dist <= InteractRadius;

        // --- DEBUG: Aviso al entrar o salir del rango ---
        if ( isCurrentlyNearby && !_playerNearby )
        {
            Log.Info( "DEBUG: Jugador ha ENTRADO en el rango de la tienda." );
        }
        else if ( !isCurrentlyNearby && _playerNearby )
        {
            Log.Info( "DEBUG: Jugador ha SALIDO del rango de la tienda." );
        }

        _playerNearby = isCurrentlyNearby;

        if (_playerNearby && Input.Pressed("Interactuar"))
        {
            var shop = Scene.GetAllComponents<Sandbox.ItemShop>().FirstOrDefault();
            if (shop != null)
            {
                Log.Info( "DEBUG: Intentando abrir tienda..." );
                shop.Open();
            }
        }
    }

    protected override void DrawGizmos()
    {
        Gizmo.Draw.Color = _playerNearby ? Color.Green : Color.Yellow;
        Gizmo.Draw.LineSphere(Vector3.Zero, InteractRadius);
    }

    public bool IsPlayerNearby() => _playerNearby;
}
