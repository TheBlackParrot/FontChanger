using FontChanger.UI;
using JetBrains.Annotations;
using Zenject;

namespace FontChanger.Installers;

[UsedImplicitly]
internal class MenuInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<SettingsMenuManager>().AsSingle();
    }
}