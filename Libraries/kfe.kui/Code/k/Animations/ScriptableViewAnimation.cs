namespace Sandbox.k.Animations {
    public class ScriptableViewAnimation<T> : ScriptableAnimationBase where T : class {
        [Property] protected T _animationParameters { get; set; }

        public override void Play(GameObject target) {
            base.Play(target);
        }
    }
}
