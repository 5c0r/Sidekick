using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Sidekick.Business.Languages.UI;
using Sidekick.Core.Natives;
using Sidekick.Core.Settings;

namespace Sidekick.UI.Settings
{
    public class SettingsViewModel : SidekickViewModel, ISettingsViewModel, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IUILanguageProvider uiLanguageProvider;
        private readonly SidekickSettings sidekickSettings;
        private readonly INativeKeyboard nativeKeyboard;

        public SettingsViewModel(IUILanguageProvider uiLanguageProvider,
            SidekickSettings sidekickSettings,
            INativeKeyboard nativeKeyboard,
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this.uiLanguageProvider = uiLanguageProvider;
            this.sidekickSettings = sidekickSettings;
            this.nativeKeyboard = nativeKeyboard;
            Settings = new SidekickSettings();
            AssignValues(sidekickSettings, Settings);

            Keybinds.Clear();
            Settings.GetType()
                .GetProperties()
                .Where(x => x.Name.StartsWith("Key"))
                .ToList()
                .ForEach(x =>
                {
                    Keybinds.Add(x.Name, x.GetValue(Settings).ToString());
                });

            WikiOptions.Add("POE Wiki", WikiSetting.PoeWiki.ToString());
            WikiOptions.Add("POE Db", WikiSetting.PoeDb.ToString());

            uiLanguageProvider.AvailableLanguages
                .ForEach(x =>
                {
                    UILanguageOptions.Add(x.DisplayName, x.Name);
                });

            nativeKeyboard.OnKeyDown += NativeKeyboard_OnKeyDown;
        }

        public SidekickSettings Settings { get; private set; }

        public Dictionary<string, string> Keybinds { get; private set; } = new Dictionary<string, string>();

        public KeyValuePair<string, string>? CurrentKeybind { get; set; }

        public Dictionary<string, string> WikiOptions { get; private set; } = new Dictionary<string, string>();

        public Dictionary<string, string> UILanguageOptions { get; private set; } = new Dictionary<string, string>();

        public void Save()
        {
            var keybindProperties = Settings.GetType().GetProperties();

            Keybinds
                .ToList()
                .ForEach(keybind =>
                {
                    keybindProperties.First(x => x.Name == keybind.Key).SetValue(Settings, keybind.Value);
                });

            AssignValues(Settings, sidekickSettings);
            uiLanguageProvider.SetLanguage(uiLanguageProvider.AvailableLanguages.FirstOrDefault(x => x.Name == Settings.UILanguage));
            sidekickSettings.Save();
        }

        public bool IsKeybindUsed(string keybind, string ignoreKey = null)
        {
            return Keybinds.Any(x => x.Value == keybind && x.Key != ignoreKey);
        }

        private static void AssignValues(SidekickSettings src, SidekickSettings dest)
        {
            // iterates through src Settings (properties) and copies them to dest settings (properties)
            // If there ever comes a time, where some properties do not have to be copied, we can add attributes to exclude them
            src.GetType().GetProperties().ToList().ForEach(x => x.SetValue(dest, x.GetValue(src)));
        }

        private Task NativeKeyboard_OnKeyDown(string key)
        {
            if (key == "Esc")
            {
                CurrentKeybind = null;
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            nativeKeyboard.OnKeyDown -= NativeKeyboard_OnKeyDown;
        }
    }
}
