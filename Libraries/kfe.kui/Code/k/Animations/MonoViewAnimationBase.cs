using Sandbox.k.Interfaces;

namespace Sandbox.k.Animations {
    /// <summary>
    /// TODO: remove MonoBehaviour, add ViewAnimation as optional parameter from code
    /// </summary>
    public class MonoViewAnimationBase : Component, IViewAnimation {
        public virtual void Play(GameObject target) {
        }
    }
}
