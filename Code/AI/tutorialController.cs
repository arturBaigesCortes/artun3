using Sandbox;
using System.Linq;

namespace Artun2;


// enlaza todos los eventos del tutorial, en una secuencia que se va abriendo al completar las tascas
public sealed class TutorialController : Component
{
    [Property] public TutorialUI TutorialUI { get; set; }
	[Property] public ItemResource TutorialCaptureBoostItem { get; set; }

	private int _fishesProcessed = 0;

    public enum TutorialStep
	{
		NotStarted,
		LaunchBobber,
		ReactToBite,
		MiniGame,
		ReelIn,
		UseItem,
		OpenShop,
		KarmaInfo,
		CancelFishing,
		OpenInventory,
		TalkToFish,
		SellOrRelease,
		FourFishes,
		Completed
	}

    public TutorialStep CurrentStep { get; private set; } = TutorialStep.NotStarted;

    protected override void OnStart()
    {
        AdvanceTo(TutorialStep.LaunchBobber);
    }

    // — Métodos públicos que llamas desde tus otros scripts —

    public void OnBobberLaunched()
    {
        if (CurrentStep != TutorialStep.LaunchBobber) return;
        TutorialUI?.CompleteStep();
        AdvanceTo(TutorialStep.ReactToBite);
    }

    public void OnFishBite()
    {
        if (CurrentStep != TutorialStep.ReactToBite) return;
        TutorialUI?.ShowStep(
            "¡El pez ha picado! Haz click izquierdo rápido para reaccionar antes de que se escape.",
            "Haz click izquierdo cuando pique"
        );
    }

    public void OnReactionSuccess()
    {
        if (CurrentStep != TutorialStep.ReactToBite) return;
        TutorialUI?.CompleteStep();
        AdvanceTo(TutorialStep.MiniGame);
    }

    public void OnMiniGameStarted()
    {
        if (CurrentStep != TutorialStep.MiniGame) return;
        TutorialUI?.ShowStep(
            "Mueve el cursor para mantener al jugador sobre el pez. ¡Llena la barra verde para atrapar!",
            "Supera el minijuego de pesca"
        );
    }

	//aqui se recibe extermanete si el paso del tutorial se ha completado
    public void OnFishCaught()
    {
        if (CurrentStep != TutorialStep.MiniGame) return;
        TutorialUI?.CompleteStep();
        AdvanceTo(TutorialStep.ReelIn);
    }

    public void OnRodReset()
    {
        if (CurrentStep != TutorialStep.ReelIn) return;
        TutorialUI?.CompleteStep();
        AdvanceTo(TutorialStep.UseItem);
    }

    public void OnItemUsed()
    {
        if (CurrentStep != TutorialStep.UseItem) return;
        TutorialUI?.CompleteStep();
        AdvanceTo(TutorialStep.OpenShop);
    }

	public void OnItemBought()
	{
		if (CurrentStep != TutorialStep.OpenShop) return;
		TutorialUI?.CompleteStep();
		AdvanceTo(TutorialStep.KarmaInfo);
	}

	// Llamado desde TutorialUI cuando el jugador cierra el mensaje con F
	public void OnKarmaInfoClosed()
	{
		if (CurrentStep != TutorialStep.KarmaInfo) return;
		TutorialUI?.CompleteStep();
		AdvanceTo(TutorialStep.CancelFishing);
	}

	public void OnFishingCancelled()
	{
		if (CurrentStep != TutorialStep.CancelFishing) return;
		TutorialUI?.CompleteStep();
		AdvanceTo(TutorialStep.OpenInventory);
	}

	public void OnInventoryOpened()
	{
		if (CurrentStep != TutorialStep.OpenInventory) return;
		TutorialUI?.CompleteStep();
		AdvanceTo(TutorialStep.TalkToFish);
	}

	public void OnTalkedToFish()
	{
		if (CurrentStep != TutorialStep.TalkToFish) return;
		TutorialUI?.CompleteStep();
		AdvanceTo(TutorialStep.SellOrRelease);
	}

	public void OnFishSoldOrReleased()
	{
		// Paso SellOrRelease — primer pez
		if (CurrentStep == TutorialStep.SellOrRelease)
		{
			_fishesProcessed = 1;
			TutorialUI?.CompleteStep();
			AdvanceTo(TutorialStep.FourFishes);
			return;
		}

		// Paso FourFishes — contar hasta 4
		if (CurrentStep == TutorialStep.FourFishes)
		{
			_fishesProcessed++;
			if (_fishesProcessed >= 4)
			{
				TutorialUI?.CompleteStep();
				AdvanceTo(TutorialStep.Completed);
			}
			else
			{
				TutorialUI?.ShowStep(
					$"¡Bien! Sigue pescando. Llevas {_fishesProcessed}/4.",
					$"Procesa 4 peces en total ({_fishesProcessed}/4)"
				);
			}
		}
	}

    // — Privado —

    private void AdvanceTo(TutorialStep step)
    {
        CurrentStep = step;

        switch (step)
        {
			//aqui se define que dice cada paso del tutorial y la mision que sale
            case TutorialStep.LaunchBobber:
                TutorialUI?.ShowStep(
                    "Pulsa Q para lanzar el anzuelo al agua. ¡Apunta hacia el mar!",
                    "Lanza el anzuelo al agua"
                );
                break;

            case TutorialStep.ReactToBite:
                TutorialUI?.ShowStep(
                    "Ahora espera a que el anzuelo se hunda. Cuando pique, ¡reacciona rápido!",
                    "Espera a que pique un pez"
                );
                break;

            case TutorialStep.ReelIn:
                TutorialUI?.ShowStep(
                    "¡Bien hecho! Pulsa Q de nuevo para recoger la caña.",
                    "Recoge la caña con Q"
                );
                break;

            case TutorialStep.UseItem:
				var inv = Scene.GetAllComponents<ItemInventory>().FirstOrDefault();
				if (TutorialCaptureBoostItem != null && inv != null)
					inv.GiveItem(TutorialCaptureBoostItem, 1);

				TutorialUI?.ShowStep(
					"¡Te dejo este objeto, no me lo rompas! Equípalo con la RUEDA DEL RATÓN y úsalo con ESPACIO durante la pesca.",
					"Usa el impulso de captura con ESPACIO durante una pesca"
				);
				break;

            case TutorialStep.OpenShop:
				Scene.GetAllComponents<Money>().FirstOrDefault()?.Add(140);
                TutorialUI?.ShowStep(
                    "Ve a la tienda del muelle para comprar nuevos items y mejorar tu equipo. Te dejo algo de dinero no te lo gastes todo de golpe.",
                    "Abre la tienda con F en el carrito del mapa y compra algo"
                );
                break;

			case TutorialStep.KarmaInfo:
				TutorialUI?.ShowStep(
					"El Karma refleja cómo tratas a los peces. Si los vendes sin hablar con ellos, bajará y las capturas serán más difíciles. ¡Habla siempre con tus capturas antes de decidir su destino!",
					"Lee sobre el sistema de Karma (F para continuar)"
				);
				break;

			case TutorialStep.CancelFishing:
				TutorialUI?.ShowStep(
					"Sabías que puedes pulsar Q con el anzuelo lanzado para cancelar y recoger la caña. Pruébalo ahora: lanza con Q y recógelo de nuevo con Q.",
					"Lanza la caña y cancélala con Q"
				);
				break;

			case TutorialStep.OpenInventory:
				TutorialUI?.ShowStep(
					"Pulsa E para abrir tu inventario y ver los peces que has pescado.",
					"Abre el inventario con E"
				);
				break;

			case TutorialStep.TalkToFish:
				TutorialUI?.ShowStep(
					"Haz click en un pez de tu inventario para hablar con él. ¡Cada pez tiene su propia personalidad!",
					"Habla con un pez del inventario"
				);
				break;

			case TutorialStep.SellOrRelease:
				TutorialUI?.ShowStep(
					"Puedes vender el pez para ganar dinero, o devolverlo al agua. ¡Tú decides!",
					"Vende o devuelve un pez"
				);
				break;

			case TutorialStep.FourFishes:
				TutorialUI?.ShowStep(
					$"¡Bien! Ahora pesca, habla y decide el destino de 4 peces en total. Llevas {_fishesProcessed}/4.",
					"Procesa 4 peces en total (vender o devolver)"
				);
				break;

            case TutorialStep.Completed:
                Log.Info("[Tutorial] ¡Tutorial completado!");
                break;
        }
    }
}
