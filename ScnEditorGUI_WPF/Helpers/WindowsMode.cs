using Microsoft.Win32;

namespace ScnEditorGUI_WPF.Helpers
{
    static class WindowsMode
	{
		private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

		private const string RegistryValueName = "AppsUseLightTheme";

		public enum Theme
		{
			Light,
			Dark
		}

		public static Theme GetTheme()
		{
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);

            object registryValueObject = key?.GetValue(RegistryValueName);
            if (registryValueObject == null)
            {
                return Theme.Light;
            }

            int registryValue = (int)registryValueObject;

            return registryValue > 0 ? Theme.Light : Theme.Dark;
        }
	}
}
