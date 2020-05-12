using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TaskbarCustomizer.TaskSettings
{

    public class Setting
    {
        public string SettingName { get; set; }
        public string SettingValue { get; set; }

        public Setting()
        {
        }

        public Setting(string SettingName, string SettingValue)
        {
            this.SettingName = SettingName;
            this.SettingValue = SettingValue;
        }
    }

    public class Settings
    {
        private List<Setting> _settingsList = new List<Setting>();

        public Settings()
        {
        }

        public void SaveSettings()
        {
            if (_settingsList.Count == 0) return;

            using (StreamWriter sw = new StreamWriter("settings.txt"))
            {
                foreach (Setting s in _settingsList)
                {
                    sw.WriteLine("{0}: {1}", s.SettingName.Trim(), s.SettingValue.Trim());
                }
            }
        }

        public bool LoadSettings()
        {
            if (File.Exists("settings.txt"))
            {
                using (StreamReader sr = new StreamReader("settings.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] temp = sr.ReadLine().Split(':');

                        Setting setting = new Setting
                        {
                            SettingName = temp[0].Trim(),
                            SettingValue = temp[1].Trim()
                        };
                        AddSetting(setting);
                    }

                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public void AddSetting(Setting Setting)
        {
            if (FindSetting(Setting.SettingName) == null)
                _settingsList.Add(Setting);
        }

        public void RemoveSetting(Setting Setting)
        {
            Setting setting = FindSetting(Setting.SettingName);

            if (setting != null)
                _settingsList.Remove(Setting);
        }

        public void UpdateSetting(Setting Setting)
        {
            int index = _settingsList.FindIndex(s => s.SettingName.ToLower() == Setting.SettingName.ToLower());

            if (index == -1) return;

            _settingsList[index] = Setting;
        }

        public Setting FindSetting(string SettingName)
        {
            return _settingsList.FirstOrDefault(s => s.SettingName.ToLower().Trim() == SettingName.ToLower().Trim());
        }
    }
}