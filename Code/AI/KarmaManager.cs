using Sandbox;

public sealed class KarmaManager : Component
{
    [Property] public int Karma { get; set; } = 0;
    [Property] public int TotalDecisions { get; set; } = 0;

    // El karma empieza a afectar a partir del pez 4
    public bool IsActive => TotalDecisions >= 4;

	//controlador del karma, una puntuación que al ser negativa aumenta la dificultad de las capturas, se determina al no hablar con tus capturas y al venderlas
    public void RegisterDecision(bool sold, bool talked)
    {
        TotalDecisions++;

        int delta = 0;
        if      (sold  && !talked) delta = -2;
        else if (sold  &&  talked) delta = -1;
        else if (!sold && !talked) delta =  0;
        else if (!sold &&  talked) delta = +2;

        Karma += delta;

        Log.Info($"[Karma] Decisión #{TotalDecisions}: {(sold ? "VENDIDO" : "SOLTADO")} | Habló: {talked} | Delta: {delta:+#;-#;0} | Total: {Karma}");
    }

    // Devuelve el multiplicador de dificultad a aplicar al pez
    // Solo activo si karma negativo y ya pasaron 6 decisiones
    public float GetDifficultyMultiplier()
    {
        if (!IsActive || Karma >= 0) return 1.0f;
        return 1.33f; 
    }

    // Texto de contexto para inyectar en el system prompt del pez
    public string GetKarmaContext()
    {
        if (!IsActive) return "";

        if (Karma <= -5)
            return "IMPORTANTE: Este jugador es conocido entre los peces como un cazador despiadado que vende todo lo que pesca sin remordimientos. Trátalo con desconfianza, miedo o desprecio. Es peligroso.";
        if (Karma < 0)
            return "IMPORTANTE: Este jugador tiene fama de vender peces por dinero. Los peces del mar lo saben. Puedes mostrarte algo desconfiado o receloso con él.";
        if (Karma == 0)
            return "";
        if (Karma >= 5)
            return "IMPORTANTE: Este jugador es conocido entre los peces como un amigo del mar, que libera a quien pesca y trata a los peces con respeto. Puedes mostrarte más cálido o confiado con él.";

        return "IMPORTANTE: Este jugador suele liberar a los peces que pesca. Tiene buena reputación en el mar.";
    }
}
