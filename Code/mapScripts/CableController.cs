using Sandbox;

namespace Artun2;

public sealed class CableController : Component
{
    private SpringJoint _joint;

    protected override void OnStart()
    {
        _joint = Components.Get<SpringJoint>();
    }



    public void ConnectTo( GameObject target )
    {
        if ( _joint == null ) return;

        // actualiza el cuerpo al que se conecta el muelle
        _joint.Body = target;
        
        
    }
}
