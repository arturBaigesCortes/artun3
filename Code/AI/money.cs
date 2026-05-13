using Sandbox;


//contea y controla el dinero del usuaio, y lo añade si es necesario
public class Money : Component
{
    [Property] public int Amount { get; set; } = 0;

    protected override void OnUpdate()
    {
        // Verificamos si se presionó la acción configurada en el Input Manager
        if ( Input.Pressed( "Debug1" ) )
        {
            Add( 1000 );
            Log.Info( $"Debug: Oro actual: {Amount}" );
        }
    }

    public void Add( int amount )
    {
        Amount += amount;
    }

    public void Spend( int amount )
    {
        Amount -= amount;
    }
}
