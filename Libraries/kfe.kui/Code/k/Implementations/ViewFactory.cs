using Sandbox.k.Interfaces;
using Sandbox.k.Views;

namespace Sandbox.k.Implementations {
    public class ViewFactory : IViewFactory {
        public virtual ViewBase CreateView(ViewBase prefab, GameObject parent)
        {
	        var instance = prefab.GameObject.Clone();
	        instance.SetParent( parent );
	        return instance.GetOrAddComponent<ViewBase>();
        }
    }
}
