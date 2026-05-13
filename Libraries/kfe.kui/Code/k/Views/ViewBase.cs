using Sandbox.k.Interfaces;
using Sandbox.k.Models;

namespace Sandbox.k.Views {
    public class ViewBase : Component {
        protected virtual void Awake() {
            Initialize();
        }

        protected virtual void OnEnable() {
        }

        protected virtual void OnDisable() {
        }

        public virtual void OnViewModelUpdate(ViewModelBase model) {
        }

        protected virtual void Initialize() {
        }

        public virtual void SetActive(bool isActive, IViewAnimation viewAnimation = null) {
            GameObject.Enabled = isActive;
            if (isActive && viewAnimation != null) viewAnimation.Play(GameObject);
        }

        public bool IsActive => GameObject.IsValid() && GameObject.Enabled;
    }
}
