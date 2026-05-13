using System.Collections.Generic;

namespace Sandbox.k.Configs {
    [GameResource("UiConfig", "kuic", "Ui configuration file.", Category = "k.UI", Icon = "checkroom")]
    public class UiConfig : GameResource {
        public List<GameObject> ViewBases { get; set; }
    }
}
