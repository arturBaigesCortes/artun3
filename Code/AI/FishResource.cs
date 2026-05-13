using Sandbox;

public enum FishAbility 
{ 
    None, 
    PhantomImage, 
    Teleport, 
    SpeedBurst 
}

//la declaración de todas las estadísticas de los peces, prompt, inteligencia, dificultad... en un .fish
[GameResource( "Fish Definition", "fish", "Datos de un pez específico" )]

public partial class FishResource : GameResource
{


	[Property, Group("Economía")]
	public int Price { get; set; } = 100; 


    [Property, Group("Visuales")] 
    public string Name { get; set; } = "Pez";

    [Property, Group("Visuales")] 
    public Model FishModel { get; set; }

    [Property, Group("Visuales"), ImageAssetPath] 
    public string IconPath { get; set; } = "textures/items/fish.png";

	[Property, Group("Visuales"), ImageAssetPath] 
    public string IconPathChat { get; set; } = "textures/items/pez_de_frente.png";

    [Property, Group("Dificultad")] 
    public float Difficulty { get; set; } = 1.0f;

    [Property, Group("Dificultad")] 
    public float CaptureTime { get; set; } = 3.0f;

    [Property, Group("Dificultad")] 
    [Description("Cuánto progreso se pierde por segundo cuando el jugador no está encima del pez")]
    public float LossRate { get; set; } = 0.12f;

    [Property, Group("IA")] 
    [Description("Velocidad de movimiento hacia los objetivos. Recomendado: 1.0 a 6.0")]
    public float EvasionSpeed { get; set; } = 2.5f;

    [Property, Group("IA")] 
    [Description("Frecuencia con la que cambia de dirección. A más valor, más errático.")]
    public float Intelligence { get; set; } = 1.0f;

    [Property, Group("IA")] 
    public float MovementRandomness { get; set; } = 0.5f;

    [Property, Group("IA"), TextArea] 
    public string Personality { get; set; } = "Eres un pez animado.";

    /// <summary>
    /// Controla la aleatoriedad de la IA.
    /// 0.0 = Muy predecible/aburrido, 2.0 = Completamente caótico.
    /// </summary>
    [Property, Group("IA"), Range( 0, 2 )] 
    public float Temperature { get; set; } = 1.0f;

    [Property, Group("IA")]
    [Description("Probabilidad de que el pez pegue un pequeño salto (dash) hacia una esquina (0 a 1)")]
    public float DashChance { get; set; } = 0.1f;


	/// <summary>
    /// Controla la aleatoriedad de la IA.
    /// 0.0 = Muy predecible/aburrido, 2.0 = Completamente caótico.
    /// </summary>
	[Property, Group("Jefe")]
	public bool IsBoss { get; set; } = false;

	[Property, Group("Jefe")]
	public FishAbility Ability { get; set; } = FishAbility.None;
}
