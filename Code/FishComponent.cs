using Sandbox;

public sealed class FishComponent : Component
{
    [Property] public string FishName { get; set; } = "Grumpy Bass";

    // We store the ident as a string. 
    // s&box will see this string during export and pull the cloud asset.
    [Property] public string MapIdent { get; set; } = "boggy.watertrainstation";

    public void OnCaught()
    {
        Log.Info( $"Log: You caught {FishName}!" );
        GameObject.Enabled = false;
    }
}
