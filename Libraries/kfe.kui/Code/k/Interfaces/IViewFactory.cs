using Sandbox.k.Views;

namespace Sandbox.k.Interfaces {
    public interface IViewFactory {
        public ViewBase CreateView(ViewBase prefab, GameObject parent);
    }
}
