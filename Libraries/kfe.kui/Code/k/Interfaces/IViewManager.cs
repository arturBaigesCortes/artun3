using System.Collections.Generic;
using Sandbox.k.Models;
using Sandbox.k.Views;

namespace Sandbox.k.Interfaces {
    // todo refactor
    public interface IViewManager {
        public void Register(string viewName, ViewBase view);
        public void Register(IEnumerable<ViewBase> views);
        public void Unregister(string viewName);
        public void CloseAllViews();

        public void SetActiveView(
            string viewName,
            bool isActive,
            ViewModelBase model = null,
            IViewAnimation animation = null);
        // move ViewModelBase and IViewAnimation into separate controller

        public T SetActiveView<T>(
            string viewName,
            bool isActive,
            ViewModelBase model = null,
            IViewAnimation animation = null
        ) where T : ViewBase;
        // move ViewModelBase and IViewAnimation into separate controller

        public bool TryGetView<T>(string name, out T view) where T : ViewBase;
        public bool IsActive(string name);
    }
}