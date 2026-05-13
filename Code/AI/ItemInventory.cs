using Sandbox;
using System; // <--- Add this line
using System.Collections.Generic;
using System.Linq;

public sealed class ItemInventory : Component
{
    public const int MaxSlots = 9;

    public List<ItemResource> OwnedItems { get; set; } = new();

    // Durabilidad actual de cada item
    public List<int> Durabilities { get; set; } = new();

    // ningún item equipado
    public int SelectedIndex { get; set; } = -1;

    public ItemResource SelectedItem =>
        (SelectedIndex >= 0 && SelectedIndex < OwnedItems.Count)
            ? OwnedItems[SelectedIndex]
            : null;

    // Si el item seleccionado está roto
    public bool SelectedIsBroken =>
        SelectedItem != null &&
        SelectedItem.MaxDurability > 0 &&
        GetDurability(SelectedIndex) <= 0;

    protected override void OnStart()
    {
        // Sin item inicial — todo se compra en la tienda
        SelectedIndex = -1;
    }

    protected override void OnUpdate()
	{
		// Buscamos el componente de pesca en la escena
		var fishingEvent = Scene.GetAllComponents<FishingEvent>().FirstOrDefault();

		// Si el minijuego está activo, salimos de la función sin procesar el cambio de ítem
		if ( fishingEvent != null && fishingEvent.IsActive )
			return;

		// Lógica original de cambio de ítem
		float scroll = Input.MouseWheel.y;
		if (scroll > 0) CycleItem(-1);
		else if (scroll < 0) CycleItem(1);
	}

    public void CycleItem(int dir)
    {
        int total = OwnedItems.Count + 1;
        int current = SelectedIndex + 1;
        current = (current + dir + total) % total;
        SelectedIndex = current - 1;

        string nombre = SelectedItem?.Name ?? "Ninguno";
        Log.Info($"[Item] Seleccionado: {nombre}");
    }

    public void OnFishingStart()
    {
        Log.Info($"[Item] Pesca iniciada con: {SelectedItem?.Name ?? "Ninguno"}");
    }

    // Llamado al terminar una pesca donde se usó el item al menos una vez
   public void OnItemUsedThisFish()
	{
		if (SelectedItem == null) return;
		if (SelectedItem.MaxDurability <= 0) return; 

		int idx = SelectedIndex;
		
		while (Durabilities.Count <= idx)
			Durabilities.Add(SelectedItem.MaxDurability);

		Durabilities[idx] = Math.Max(0, Durabilities[idx] - 1);

		Log.Info($"[Item] Durabilidad de {SelectedItem.Name}: {Durabilities[idx]}/{SelectedItem.MaxDurability}");

		// Avisar al tutorial que se usó un item
		Scene.GetAllComponents<Artun2.TutorialController>().FirstOrDefault()?.OnItemUsed();

		if (Durabilities[idx] <= 0)
		{
			Log.Warning($"[Item] {SelectedItem.Name} se ha roto y ha sido eliminado.");
			OwnedItems.RemoveAt(idx);
			Durabilities.RemoveAt(idx);
			SelectedIndex = -1; 
		}
	}

    public int GetDurability(int index)
	{
		if (index < 0 || index >= OwnedItems.Count) return 0;
		
		// Si la lista de durabilidades es más corta que la de items, la sincronizamos
		if (Durabilities.Count <= index)
		{
			while (Durabilities.Count <= index)
				Durabilities.Add(OwnedItems[Durabilities.Count].MaxDurability);
		}
		
		return Durabilities[index];
	}


    public bool Buy(ItemResource item)
    {
        if (OwnedItems.Count >= MaxSlots) return false;
        var money = Scene.GetAllComponents<Money>().FirstOrDefault();
        if (money == null || money.Amount < item.Price) return false;
        if (OwnedItems.Contains(item)) return false;

        money.Spend(item.Price);
        OwnedItems.Add(item);
        Durabilities.Add(item.MaxDurability); // inicializar durabilidad
        Log.Info($"[Shop] Comprado: {item.Name} | Durabilidad: {item.MaxDurability}");
        return true;
    }

	// Dar un item gratis con durabilidad personalizada (para tutorial)
	public bool GiveItem(ItemResource item, int durability = 1)
		{
			if (OwnedItems.Count >= MaxSlots) return false;
			if (OwnedItems.Contains(item)) return false;

			OwnedItems.Add(item);
			Durabilities.Add(durability);
			Log.Info($"[Tutorial] Item otorgado: {item.Name} | Durabilidad: {durability}");
			return true;
		}
}
