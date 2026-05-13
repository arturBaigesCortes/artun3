using System.Collections.Generic;
using Sandbox.k.Animations;
using Sandbox.k.Configs;
using Sandbox.k.Enums;
using Sandbox.k.Implementations;
using Sandbox.k.Interfaces;
using Sandbox.k.Models;
using Sandbox.k.Views;

namespace Sandbox.k {
    public class Ui {
        public static Ui Instance => _instance ??= new Ui();
        private static Ui _instance;

        private IViewManager _viewManager;

        public void Initialize(UiConfig uiConfig, IViewFactory viewFactory) {
            if (uiConfig == null) {
	            Log.Error($"{nameof(uiConfig)} is null");
                return;
            }

            if (viewFactory == null) {
                Log.Error($"{nameof(IViewFactory)} is null");
                return;
            }

            if (uiConfig.ViewBases == null) {
	            Log.Error( "ViewBases is null" );
				return;
			}
            
            var viewBases = new List<ViewBase>();
            foreach ( var yeah in uiConfig.ViewBases )
            {
	            viewBases.Add( yeah.GetComponent<ViewBase>() );
            }

            var canvasGo = new GameObject(null, name: "[UI] Canvas") { Flags = GameObjectFlags.DontDestroyOnLoad };
            
            _viewManager = new ViewManager(canvasGo, viewFactory, viewBases);
            _instance = this;
        }

        public static T Open<T>(
            ViewModelBase model = null,
            ViewLoadingMode loadingMode = ViewLoadingMode.Additive,
            ScriptableAnimationBase animation = null
        ) where T : ViewBase {
            if (loadingMode == ViewLoadingMode.Single) CloseAllViews();

            var name = typeof(T).Name;
            return Instance._viewManager.SetActiveView<T>(name, true, model, animation);
        }

        public static void Close<T>() where T : ViewBase {
            var name = typeof(T).Name;
            Instance._viewManager.SetActiveView(name, false);
        }

        public static T SetActive<T>(
            bool isActive,
            ViewModelBase model = null,
            IViewAnimation animation = null
        ) where T : ViewBase {
            var name = typeof(T).Name;
            return Instance._viewManager.SetActiveView<T>(name, isActive, model, animation);
        }

        public static void SetActive(string viewName, bool isActive, ViewModelBase model = null) {
            Instance._viewManager.SetActiveView(viewName, isActive, model);
        }

        public static bool TryGet<T>(out T view) where T : ViewBase {
            var name = typeof(T).Name;
            return Instance._viewManager.TryGetView(name, out view);
        }

        public static bool IsActive<T>() where T : ViewBase {
            var viewName = typeof(T).Name;
            return IsActive(viewName);
        }

        public static bool IsActive(string viewName) {
            return Instance._viewManager.IsActive(viewName);
        }

        private static void CloseAllViews() {
            Instance._viewManager.CloseAllViews();
        }
    }
}
