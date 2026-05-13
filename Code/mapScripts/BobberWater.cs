using Sandbox;
using System;


public sealed class BobberWater : Component, Component.ITriggerListener
{
    [Property] public float WaterLevel { get; set; } = 26.0f; 
    
    
    [Property] public Artun2.CableController Cable { get; set; } 
    
    [Property] public GameObject PuntaCaña { get; set; } 

    public enum BobberState { Flying, Waiting, Biting }
    public BobberState State { get; private set; } = BobberState.Flying;

    private Rigidbody rb;
    private float biteTimer = 0f;
    private float timeUntilBite = 0f;

    protected override void OnStart() => rb = Components.Get<Rigidbody>();

    void ITriggerListener.OnTriggerEnter( Collider other ) 
    {
        if ( other.GameObject.Tags.Has( "water" ) ) EntrarEnAgua();
    }

    private void EntrarEnAgua()
    {
        if ( State != BobberState.Flying ) return;
        State = BobberState.Waiting;
        biteTimer = 0f;
        timeUntilBite = Game.Random.Float( 3f, 10f );
		//if ( rb != null ) rb.MotionEnabled = false;

		Sound.Play( "sounds/water/water_splash_medium.sound", WorldPosition );
    }

    protected override void OnUpdate()
    {

        if ( State == BobberState.Flying && WorldPosition.z <= WaterLevel ) EntrarEnAgua();

        // Lógica de pesca 
        if ( State == BobberState.Waiting )
        {
            WorldPosition = WorldPosition.WithZ( WaterLevel + MathF.Sin( Time.Now * 2.0f ) * 2.0f );
            biteTimer += Time.Delta;
            if ( biteTimer >= timeUntilBite ) State = BobberState.Biting;
        }
        if ( State == BobberState.Biting )
        {
            WorldPosition = WorldPosition.WithZ( WaterLevel - 5.0f + MathF.Sin( Time.Now * 20.0f ) * 5.0f );
        }

        if ( State == BobberState.Flying && rb != null )
		{
			rb.MotionEnabled = true;
			
			// Si se ha quedado quieto, darle un pequeño impulso para que siga
			if ( rb.Velocity.Length < 0.5f )
			{
				rb.ApplyForce( new Vector3( 0, 0, -50f ) ); // pequeño empujón hacia abajo
			}
		}
    }

	public void ResetState()
{
    State = BobberState.Flying;
    biteTimer = 0f;
    timeUntilBite = 0f;
}

    public bool IsBiting() => State == BobberState.Biting;
    public bool IsWaiting() => State == BobberState.Waiting;
}
