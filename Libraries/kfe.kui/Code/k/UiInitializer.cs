using Sandbox.k.Configs;
using Sandbox.k.Implementations;

namespace Sandbox.k;

[Title( nameof(UiInitializer) )]
[Category( "k.Ui" )]
public class UiInitializer : Component
{
	[Property] private UiConfig _config { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Ui.Instance.Initialize(_config, new ViewFactory());
	}
}
