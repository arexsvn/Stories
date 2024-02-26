using UnityEngine;
using VContainer;
using VContainer.Unity;

public class UserInstaller : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        builder.Register<IUserApi, UserApi>(Lifetime.Singleton);
    }
}
