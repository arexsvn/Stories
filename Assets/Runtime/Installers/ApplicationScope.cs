using VContainer;
using VContainer.Unity;

public class ApplicationScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<GameController>();

        builder.Register<UICreator>(Lifetime.Singleton);
        builder.Register<MainMenuController>(Lifetime.Singleton);
        builder.Register<UIController>(Lifetime.Singleton);
        builder.Register<SceneController>(Lifetime.Singleton);
        builder.Register<HudController>(Lifetime.Singleton);
        builder.Register<TextOverlayController>(Lifetime.Singleton);
        builder.Register<DialogueController>(Lifetime.Singleton);
        builder.Register<DialogueParser>(Lifetime.Singleton);
        builder.Register<LocaleManager>(Lifetime.Singleton);
        builder.Register<AssetsManifest>(Lifetime.Singleton);
        builder.Register<CharacterManager>(Lifetime.Singleton);
        builder.Register<MemoryController>(Lifetime.Singleton);
        builder.Register<MemoryParser>(Lifetime.Singleton);
        builder.Register<JournalController>(Lifetime.Singleton);
        builder.Register<SaveStateController>(Lifetime.Singleton);
        builder.Register<InboxController>(Lifetime.Singleton);
        builder.Register<CameraController>(Lifetime.Singleton);
        builder.Register<ClockController>(Lifetime.Singleton);
        builder.Register<WebRequestService>(Lifetime.Singleton);
        builder.Register<ConnectionConfiguration>(Lifetime.Singleton);
        builder.Register<JsonSerializationOption>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<BasicDialogController>(Lifetime.Singleton);
        builder.Register<AddressablesAssetService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        // Message Channels
        builder.Register<MessageChannel<ApplicationMessage>>(Lifetime.Singleton).AsImplementedInterfaces();

        new DynamicGameObjectInstaller().Install(builder);
        new UserInstaller().Install(builder);
    }
}