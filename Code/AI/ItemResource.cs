using Sandbox;

public enum ItemType
{
    None,
    Dash,
    CaptureBoost,
    PassiveArea
}
//Este script define las estadisticas de los items .objeto, cada tipo de item tiene unas caracteristicas editables si elijes el tipo de item
[GameResource("Fishing Item", "objeto", "Objeto de pesca")]
public partial class ItemResource : GameResource
{
    [Property, Group("General")]
    public string Name { get; set; } = "Objeto";

    [Property, Group("General")]
    public string Description { get; set; } = "";

    [Property, Group("General"), ImageAssetPath]
    public string IconPath { get; set; } = "";

    [Property, Group("General")]
    public int Price { get; set; } = 50;

    [Property, Group("General")]
    public ItemType Type { get; set; } = ItemType.Dash;

    [Property, Group("General")]
    [Description("Usos (peces) antes de que el objeto se rompa. 0 = infinito.")]
    public int MaxDurability { get; set; } = 10;

    // Dash
    [Property, Group("Dash"), ShowIf("Type", ItemType.Dash)]
    public float SprintSpeedMultiplier { get; set; } = 1.8f;

    [Property, Group("Dash"), ShowIf("Type", ItemType.Dash)]
    public float SprintDuration { get; set; } = 2.0f;

    // CaptureBoost
    [Property, Group("Captura Parcial"), ShowIf("Type", ItemType.CaptureBoost)]
    public float CaptureBoostAmount { get; set; } = 0.25f;

    [Property, Group("Captura Parcial"), ShowIf("Type", ItemType.CaptureBoost)]
    public float RechargeTime { get; set; } = 4.0f;

    // PassiveArea
    [Property, Group("Área Pasiva"), ShowIf("Type", ItemType.PassiveArea)]
    public float AreaMultiplier { get; set; } = 1.5f;
}
