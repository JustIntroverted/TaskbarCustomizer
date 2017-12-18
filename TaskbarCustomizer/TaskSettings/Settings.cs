using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TaskbarCustomizer.TaskSettings {

    public class Setting {
        public string SettingName { get; set; }
        public string SettingValue { get; set; }

        public Setting() {
        }

        public Setting(string SettingName, string SettingValue) {
            this.SettingName = SettingName;
            this.SettingValue = SettingValue;
        }
    }

    public class Settings {
        private List<Setting> _settingsList = new List<Setting>();

        public Settings() {
        }

        public void SaveSettings() {
            if (_settingsList.Count == 0) return;

            using (StreamWriter sw = new StreamWriter("settings.txt")) {
                foreach (Setting s in _settingsList) {
                    sw.WriteLine("{0},{1}", s.SettingName, s.SettingValue);
                }
            }
        }

        public bool LoadSettings() {
            if (File.Exists("settings.txt")) {
                using (StreamReader sr = new StreamReader("settings.txt")) {
                    while (!sr.EndOfStream) {
                        string[] temp = sr.ReadLine().Split(',');

                        Setting setting = new Setting {
                            SettingName = temp[0],
                            SettingValue = temp[1]
                        };
                        AddSetting(setting);
                    }

                    return true;
                }
            } else {
                return false;
            }
        }

        public void AddSetting(Setting Setting) {
            if (FindSetting(Setting.SettingName) == null)
                _settingsList.Add(Setting);
        }

        public void RemoveSetting(Setting Setting) {
            Setting setting = FindSetting(Setting.SettingName);

            if (setting != null)
                _settingsList.Remove(Setting);
        }

        public void UpdateSetting(Setting Setting) {
            Setting setting = FindSetting(Setting.SettingName);

            if (setting != null)
                setting = Setting;
        }

        public Setting FindSetting(string SettingName) {
            return _settingsList.FirstOrDefault(s => s.SettingName.ToLower() == SettingName.ToLower());
        }
    }
}