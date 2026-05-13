using Sandbox;
using System;
using System.Linq;
using Artun2;

public sealed class FishingEvent : Component
{

	//todas las declaraciones de diferentes stats y info externa
    [Property] public FishResource CurrentFish { get; set; }
    [Property] public GameObject FishingUIPanel { get; set; }

    [Header("Referencias de Escena")]
    [Property] public GameObject PlayerObject { get; set; }

    public bool IsActive { get; set; } = false;
    public Vector2 PlayerPos { get; private set; } = new Vector2(0.5f, 0.5f);
    public Vector2 FishPos { get; private set; } = new Vector2(0.5f, 0.5f);

    private Vector2 fishTargetPos = new Vector2(0.5f, 0.5f);
    private float nextAiTick = 0;

    public float CaptureProgress { get; private set; } = 0.4f;

    [Header("Ajustes de Velocidad")]
    public float BasePlayerSpeed { get; set; } = 0.8f;

    // Estamina genérica — su significado depende del item equipado
    public float StaminaEnergy { get; private set; } = 1.0f;
    public bool IsExhausted { get; private set; } = false;
    public bool IsSprinting { get; private set; } = false; // solo relevante con Dash

    // ¿Debe mostrarse la barra de estamina? Solo con items activos
    public bool ShowStamina { get; private set; } = false;

    private float overlapRadius = 0.14f;

    public Vector2 ClonePos { get; private set; }
    public bool IsCloneActive { get; private set; }
    private float abilityTimer = 0;
    public float CloneLifeTimer { get; private set; } = 0;
    public bool IsPreBlinking { get; private set; } = false;

    private float _fishEvasionSpeed;
    private float _fishIntelligence;
    private float _fishCaptureTime;

	private bool _itemUsedThisFish = false;

	private Action _onFishingFinished;

	
    protected override void OnUpdate()
    {
        if (Input.Pressed("Debug2"))
        {
            if (!IsActive) StartFishing();
            else StopFishing();
        }

        if (!IsActive) return;

        UpdateStaminaVisibility();
        HandleActiveItem();
        HandleMovement();
        HandleFishAI();
        HandleProgress();
    }

	public void StartFishingExternal(Action onFinished)
	{
		_onFishingFinished = onFinished;
		StartFishing();
	}

    // Decide si la barra se muestra y qué tipo de item está activo
    private void UpdateStaminaVisibility()
    {
        var item = GetSelectedItem();
        ShowStamina = item?.Type == ItemType.Dash || item?.Type == ItemType.CaptureBoost;
    }

    // Gestiona la lógica de estamina según el item equipado
    private void HandleActiveItem()
    {
        var item = GetSelectedItem();

        if (item?.Type == ItemType.Dash)
        {
            HandleDash(item);
        }
        else if (item?.Type == ItemType.CaptureBoost)
        {
            HandleCaptureBoostRecharge();
        }
        else
        {
            // Sin item activo: la estamina se recarga sola por si acaso
            IsSprinting = false;
            StaminaEnergy = Math.Min(1.0f, StaminaEnergy + Time.Delta * 0.5f);
        }
    }

	//el objeto que permite dashear, cambia las estadisticas del cursor al darle al shift
    private void HandleDash(ItemResource item)
    {
        bool sprintInput = Input.Down("Run");
        float sprintMult = item.SprintSpeedMultiplier;

        if (sprintInput && StaminaEnergy > 0 && !IsExhausted)
		{
			IsSprinting = true;
			_itemUsedThisFish = true; 
			StaminaEnergy -= Time.Delta / 2.0f;
			if (StaminaEnergy <= 0) { StaminaEnergy = 0; IsExhausted = true; }
		}
        else
        {
            IsSprinting = false;
            float recoveryRate = IsExhausted ? 0.2f : 0.5f;
            StaminaEnergy = Math.Min(1.0f, StaminaEnergy + Time.Delta * recoveryRate);
            if (StaminaEnergy >= 1.0f) IsExhausted = false;
        }
    }


	
    private void HandleCaptureBoostRecharge()
    {
        IsSprinting = false;

        // Si la estamina está agotada, recarga progresivamente
        if (IsExhausted)
        {
            var item = GetSelectedItem();
            float rechargeRate = 1.0f / (item?.RechargeTime ?? 4.0f);
            StaminaEnergy = Math.Min(1.0f, StaminaEnergy + Time.Delta * rechargeRate);
            if (StaminaEnergy >= 1.0f)
            {
                StaminaEnergy = 1.0f;
                IsExhausted = false;
                Log.Info("[Item] CaptureBoost recargado.");
            }
        }
    }


	// movimiento base del cursor
    private void HandleMovement()
    {
        var item = GetSelectedItem();
        float sprintMult = (item?.Type == ItemType.Dash && IsSprinting) ? item.SprintSpeedMultiplier : 1.0f;
        float currentSpeed = BasePlayerSpeed * sprintMult;

        Vector2 inputDir = new Vector2(
            (Input.Down("Right") ? 1 : 0) - (Input.Down("Left") ? 1 : 0),
            (Input.Down("Backward") ? 1 : 0) - (Input.Down("Forward") ? 1 : 0)
        ).Normal;

        PlayerPos += inputDir * Time.Delta * currentSpeed;
        PlayerPos = PlayerPos.Clamp(0, 1);
    }



    private void HandleFishAI()
	{
		if (CurrentFish == null) return;

		// --- LÓGICA DE DECISIÓN ---
		if (Time.Now > nextAiTick)
		{
			float intelligenceFactor = CurrentFish.Intelligence * CurrentFish.Temperature;
			// Reducimos el divisor para que las decisiones sean más frecuentes
			float decisionDelay = 1.0f / Math.Max(intelligenceFactor, 0.1f);
			
			// Si el pez es Boss, decidimos todavía más rápido
			if (CurrentFish.IsBoss) decisionDelay *= 0.5f;

			nextAiTick = Time.Now + (Random.Shared.NextSingle() * decisionDelay);

			bool isDashing = Random.Shared.NextSingle() < CurrentFish.DashChance;
			float margin = isDashing ? 0.01f : 0.02f;

			// MEJORA: Forzar a que el pez no elija una posición demasiado cercana a la actual
			Vector2 newTarget;
			int attempts = 0;
			do
			{
				newTarget = new Vector2(
					Random.Shared.NextSingle() * (1f - margin * 2f) + margin,
					Random.Shared.NextSingle() * (1f - margin * 2f) + margin
				);
				attempts++;
			} while (Vector2.Distance(FishPos, newTarget) < 0.25f && attempts < 5); // 0.25f asegura que se mueva

			fishTargetPos = newTarget;
		}

		// --- LÓGICA DE MOVIMIENTO ---
		float speed = CurrentFish.EvasionSpeed;
		
		// El wobble (temblor) lo hacemos un poco más dinámico
		Vector2 wobble = new Vector2(
			MathF.Sin(Time.Now * 15f) * 0.02f,
			MathF.Cos(Time.Now * 12f) * 0.02f
		);

		FishPos = Vector2.Lerp(FishPos, fishTargetPos + wobble, Time.Delta * speed);
		//FishPos = FishPos.Clamp(0.05f, 0.95f); // margen para que no pegue en el borde

		// --- AJUSTE DE HITBOX (15% más pequeño) ---
		float adjustedHitbox = 0.15f;

		// --- HABILIDADES ESPECIALES (BOSS) ---
		if (CurrentFish.IsBoss && CurrentFish.Ability == FishAbility.PhantomImage)
		{
			abilityTimer += Time.Delta;

			if (!IsCloneActive && abilityTimer > 5.5f && abilityTimer <= 6.0f)
				IsPreBlinking = true;

			if (!IsCloneActive && abilityTimer > 6.0f)
			{
				IsPreBlinking = false;
				IsCloneActive = true;
				abilityTimer = 0;
				CloneLifeTimer = 0;

				FishPos = new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle());
				ClonePos = new Vector2(1f - FishPos.x, 1f - FishPos.y);
				fishTargetPos = new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle());
			}

			if (IsCloneActive)
			{
				CloneLifeTimer += Time.Delta;

				Vector2 cloneTarget = new Vector2(1f - fishTargetPos.x, 1f - fishTargetPos.y);
				ClonePos = Vector2.Lerp(ClonePos, cloneTarget + (wobble * -1.5f), Time.Delta * speed);

				// Aplicamos la hitbox ajustada también aquí
				bool playerOnReal  = Vector2.Distance(PlayerPos, FishPos)  < adjustedHitbox;
				bool playerOnClone = Vector2.Distance(PlayerPos, ClonePos) < adjustedHitbox;

				if ((playerOnClone && CloneLifeTimer > 1.2f) || (playerOnReal && CloneLifeTimer > 3.0f))
				{
					IsCloneActive = false;
					abilityTimer = -2.0f;
				}
			}
		}
		else
		{
			IsCloneActive = false;
			IsPreBlinking = false;
		}
	}


private void HandleProgress()
	{
		var item = GetSelectedItem();

		// Lógica para Items Pasivos
		float areaMult = 1.0f;
		if (item?.Type == ItemType.PassiveArea)
		{
			areaMult = item.AreaMultiplier;
			// uso automático por el simple hecho de tenerlo equipado
			_itemUsedThisFish = true; 
		}

		bool isOverlapping = Vector2.Distance(PlayerPos, FishPos) < (overlapRadius * areaMult);

		float gainSpeed = Time.Delta / (CurrentFish.CaptureTime * 1.5f);
		float lossSpeed = Time.Delta * 0.08f;
		CaptureProgress += isOverlapping ? gainSpeed : -lossSpeed;

		// CaptureBoost: Espacio gasta la estamina de golpe y da impulso
		if (Input.Pressed("Jump") && item?.Type == ItemType.CaptureBoost && !IsExhausted)
		{
			CaptureProgress = Math.Min(CaptureProgress + item.CaptureBoostAmount, 0.99f);
			StaminaEnergy = 0f;
			IsExhausted = true;
			_itemUsedThisFish = true; // marcar uso
			Log.Info($"[Item] CaptureBoost usado: +{item.CaptureBoostAmount * 100}%");
		}

		if (CaptureProgress >= 1.0f) Win();
		if (CaptureProgress <= 0f) Lose();
	}

    // Helper para no repetir GetAllComponents en cada método
    private ItemResource GetSelectedItem()
    {
        return Scene.GetAllComponents<ItemInventory>().FirstOrDefault()?.SelectedItem;
    }


//inicia la pesca con un progreso inicial fijo, y busca un pez aleatorio de todos los .fish
private void StartFishing()
	{
		IsActive = true;
		CaptureProgress = 0.4f;
		StaminaEnergy = 1.0f;
		IsExhausted = false;
		IsSprinting = false;
		_itemUsedThisFish = false;

		var itemInv = Scene.GetAllComponents<ItemInventory>().FirstOrDefault();
		itemInv?.OnFishingStart();


		var allFishes = ResourceLibrary.GetAll<FishResource>().ToList();
		Log.Info($"[Pesca] Recursos encontrados: {allFishes.Count}");

		if (allFishes.Count == 0)
		{
			Log.Warning("No se han encontrado recursos .fish — abortando pesca.");
			IsActive = false;
			_onFishingFinished?.Invoke();
			_onFishingFinished = null;
			return;
		}

		int randomIndex = Random.Shared.Next(0, allFishes.Count);
		CurrentFish = allFishes[randomIndex];
		Log.Info($"¡Un {CurrentFish.Name} ha picado el anzuelo!");

		_fishEvasionSpeed = CurrentFish.EvasionSpeed;
		_fishIntelligence = CurrentFish.Intelligence;
		_fishCaptureTime  = CurrentFish.CaptureTime;

		var karma = Scene.GetAllComponents<KarmaManager>().FirstOrDefault();
		float mult = karma?.GetDifficultyMultiplier() ?? 1.0f;
		if (mult > 1.0f)
		{
			CurrentFish.EvasionSpeed = _fishEvasionSpeed * mult;
			CurrentFish.Intelligence = _fishIntelligence * mult;
			CurrentFish.CaptureTime  = _fishCaptureTime  / mult;
			Log.Warning($"[Karma] Stats aumentadas x{mult} | Karma: {karma.Karma}");
		}

		PlayerPos = new Vector2(0.5f, 0.5f);
		FishPos = new Vector2(0.5f, 0.5f);

		if (FishingUIPanel is not null) FishingUIPanel.Enabled = true;
		Mouse.Visible = true;
		SetPlayerState(false);

		Scene.GetAllComponents<TutorialController>().FirstOrDefault()?.OnMiniGameStarted(); 
	}


	//Añade el pez al inventario y para la pesca
    private void Win()
    {
        var playerInventory = Scene.GetAllComponents<Inventory>().FirstOrDefault();
        if (playerInventory != null && CurrentFish != null)
        {
            if (playerInventory.CaughtFishes.Count < 16)
            {
                playerInventory.CaughtFishes.Add(CurrentFish);
                Log.Info($"¡Has pescado un {CurrentFish.Name}!");
				Scene.GetAllComponents<TutorialController>().FirstOrDefault()?.OnFishCaught();
            }
            else
            {
                Log.Warning("¡Inventario lleno! El pez se escapó.");
				
            }
        }
        StopFishing();
		
    }


	//se escapa el pez y para la pesca
    private void Lose()
    {
        Log.Info("¡El pez se escapó!");
        StopFishing();
    }


	//logica para parar de pescar, resetea las stats que se han estado ajustando y cierra la ventana y permite moverse de nuevo
    private void StopFishing()
	{
		if (_itemUsedThisFish)
		{
			var itemInv = Scene.GetAllComponents<ItemInventory>().FirstOrDefault();
			itemInv?.OnItemUsedThisFish();
			_itemUsedThisFish = false;
		}

		if (CurrentFish != null)
		{
			CurrentFish.EvasionSpeed = _fishEvasionSpeed;
			CurrentFish.Intelligence = _fishIntelligence;
			CurrentFish.CaptureTime  = _fishCaptureTime;
		}

		IsActive = false;
		IsSprinting = false;
		if (FishingUIPanel is not null) FishingUIPanel.Enabled = false;
		Mouse.Visible = false;
		SetPlayerState(true);

		// Avisar a la caña para que recoja el bobber
		_onFishingFinished?.Invoke();
		_onFishingFinished = null;
	}


	
    private void SetPlayerState(bool state)
    {
        if (PlayerObject is null) return;
        var components = PlayerObject.Components.GetAll<Component>(FindMode.EverythingInSelfAndDescendants);
        foreach (var comp in components)
        {
            if (comp is CameraComponent || comp is FishingEvent) continue;
            var name = comp.GetType().Name.ToLower();
            if (name.Contains("controller") || name.Contains("player") || name.Contains("move") || name.Contains("look"))
                comp.Enabled = state;
        }
    }

    protected override void OnDisabled()
    {
        StopFishing();
    }
}
