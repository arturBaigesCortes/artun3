using Sandbox;

public sealed class FishingRod : Component
{
	[Property] public float MaxDistance { get; set; } = 1000f;

	protected override void OnUpdate()
	{
		// Press Left Mouse Button to "Cast"
		if ( Input.Pressed( "attack1" ) )
		{
			CastLine();
		}
	}

	void CastLine()
	{
		// This shoots a line from the center of your screen
		var ray = Scene.Camera.ScreenNormalToRay( 0.5f );
		var tr = Scene.Trace.Ray( ray, MaxDistance )
			.WithoutTags( "player" )
			.Run();

		if ( tr.Hit )
		{
			// Try to find the FishComponent on whatever we hit
			if ( tr.GameObject.Components.TryGet<FishComponent>( out var fish ) )
			{
				fish.OnCaught();
			}
			else
			{
				Log.Info( $"Hit {tr.GameObject.Name}, but no fish here." );
			}
		}
	}
}
