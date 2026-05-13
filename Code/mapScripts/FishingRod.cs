using Sandbox;
using System;
using System.Linq;

namespace Artun2;

public sealed class FishingRod : Component
{
    [Property] public GameObject BobberPrefab { get; set; }
    [Property] public GameObject VisualBobber { get; set; }
    [Property] public GameObject TipPoint { get; set; }
    [Property] public float LaunchForce { get; set; } = 800f;
    [Property] public GameObject CableObject { get; set; }

    [Property] public float ReactionWindow { get; set; } = 1.5f;

    private GameObject currentBobber;
    private BobberWater currentBobberWater;
    private float reactionTimer = 0f;
    private bool waitingForReaction = false;

    private float _originalMinLength;
    private float _originalMaxLength;
    private float _originalRestLength;
	

    protected override void OnStart()
    {
        var joint = TipPoint?.Components.Get<SpringJoint>();
        if (joint != null)
        {
            _originalMinLength = joint.MinLength;
            _originalMaxLength = joint.MaxLength;
            _originalRestLength = joint.RestLength;
        }
    }

    protected override void OnUpdate()
    {
        var fishingEvent = Scene.GetAllComponents<FishingEvent>().FirstOrDefault();
        if (fishingEvent != null && fishingEvent.IsActive) return;

        // Menu = lanzar o recoger
        if (Input.Pressed("menu")){
			if (currentBobber == null)
			{
				LaunchBobber();
			}
			else
			{
				Scene.GetAllComponents<Artun2.TutorialController>().FirstOrDefault()?.OnFishingCancelled();
				ResetFishing();
			}
		}

        // Click izquierdo = reaccionar cuando pica
        if (Input.Pressed("attack1") && currentBobber != null)
        {
            if (currentBobberWater != null && currentBobberWater.IsBiting())
            {
                waitingForReaction = false;
                Log.Info("[Caña] ¡Reacción a tiempo! Iniciando minijuego...");
				Scene.GetAllComponents<TutorialController>().FirstOrDefault()?.OnReactionSuccess();

                if (fishingEvent != null)
                    fishingEvent.StartFishingExternal(() => ResetFishing());
                else
                    ResetFishing();
            }
        }

        if (waitingForReaction)
        {
            reactionTimer += Time.Delta;
            if (reactionTimer >= ReactionWindow)
            {
                Log.Info("[Caña] Demasiado tarde, el pez escapó.");
                ResetFishing();
            }
        }
    }

    protected override void OnFixedUpdate()
    {
        if (currentBobberWater != null && currentBobberWater.IsBiting() && !waitingForReaction)
        {
            waitingForReaction = true;
            reactionTimer = 0f;
            Log.Info($"[Caña] ¡Pica! Tienes {ReactionWindow}s para reaccionar (click izquierdo).");
        }
    }

    void LaunchBobber()
    {
        if (VisualBobber == null || TipPoint == null) return;

        // Soltar el hilo para que el bobber pueda volar
        var joint = TipPoint?.Components.Get<SpringJoint>();
        if (joint != null)
        {
            joint.MinLength = 0f;
            joint.MaxLength = 20000f;
            joint.RestLength = 10000f;
        }

        var rb = VisualBobber.Components.Get<Rigidbody>();
        if (rb != null)
        {
            rb.MotionEnabled = true;
            rb.LinearDamping = 2f;
            rb.AngularDamping = 1.0f;

            Vector3 direction = (TipPoint.Transform.Rotation.Forward + Vector3.Up * 0.5f).Normal;
            rb.Velocity = direction * LaunchForce;
        }

        currentBobber = VisualBobber;
        currentBobberWater = VisualBobber.Components.Get<BobberWater>();

        waitingForReaction = false;
        reactionTimer = 0f;
        Log.Info("[Caña] Bobber lanzado.");
		Scene.GetAllComponents<TutorialController>().FirstOrDefault()?.OnBobberLaunched();
    }

    public void ResetFishing()
{
    waitingForReaction = false;
    reactionTimer = 0f;
    currentBobberWater?.ResetState(); // resetear el estado del bobber
    currentBobberWater = null;
    currentBobber = null;

    // Restaurar hilo
    var joint = TipPoint?.Components.Get<SpringJoint>();
    if (joint != null)
    {
        joint.MinLength = _originalMinLength;
        joint.MaxLength = _originalMaxLength;
        joint.RestLength = _originalRestLength;
    }

    var rb = VisualBobber.Components.Get<Rigidbody>();
    if (rb != null)
    {
        // matar toda velocidad
        rb.Velocity = Vector3.Zero;
        rb.AngularVelocity = Vector3.Zero;
        // deshabilitar física
        rb.MotionEnabled = false;
    }

    // Posiciona despues de desactivar la física
    VisualBobber.Transform.Position = TipPoint.Transform.Position;
    VisualBobber.Transform.Rotation = TipPoint.Transform.Rotation;

    Log.Info("[Caña] Caña reseteada.");


	Scene.GetAllComponents<TutorialController>().FirstOrDefault()?.OnRodReset();
}
}
