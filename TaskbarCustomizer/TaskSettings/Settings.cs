using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TaskbarCustomizer.TaskSettings
{

    public struct Setting
    {
        public string SettingName { get; set; }
        public string SettingValue { get; set; }

        public Setting(string SettingName, string SettingValue)
        {
            this.SettingName = SettingName;
            this.SettingValue = SettingValue;
        }
    }

    public class Settings
    {
        public List<Setting> TBCustomizerSettings { get; set; } = new List<Setting>();

        public Settings()
        {
        }

        public void SaveSettings()
        {
            if (TBCustomizerSettings.Count == 0) return;

            string jsonString = JsonConvert.SerializeObject(TBCustomizerSettings, Formatting.Indented);
            File.WriteAllText("settings.json", jsonString);

            #region old saving scope
            //using (StreamWriter sw = new StreamWriter("settings.txt"))
            //{
            //    foreach (Setting s in _settingsList)
            //    {
            //        sw.WriteLine("{0}: {1}", s.SettingName.Trim(), s.SettingValue.Trim());
            //    }
            //}
            #endregion
        }

        public void LoadSettings()
        {
            if (File.Exists("settings.json"))
            {
                string jsonString = File.ReadAllText("settings.json");
                TBCustomizerSettings = JsonConvert.DeserializeObject<List<Setting>>(jsonString);
            }

            #region old loadings scope
            //if (File.Exists("settings.txt"))
            //{
            //    using (StreamReader sr = new StreamReader("settings.txt"))
            //    {
            //        while (!sr.EndOfStream)
            //        {
            //            string[] temp = sr.ReadLine().Split(':');

            //            Setting setting = new Setting
            //            {
            //                SettingName = temp[0].Trim(),
            //                SettingValue = temp[1].Trim()
            //            };
            //            AddSetting(setting);
            //        }

            //        return true;
            //    }
            //}
            //else
            //{
            //    return false;
            //}
            #endregion
        }

        public void AddSetting(Setting Setting)
        {
            if (!FindSetting(Setting.SettingName).Equals(null))
                TBCustomizerSettings.Add(Setting);
        }

        public void RemoveSetting(Setting Setting)
        {
            Setting setting = FindSetting(Setting.SettingName);

            if (!setting.Equals(null))
                TBCustomizerSettings.Remove(Setting);
        }

        public void UpdateSetting(Setting Setting)
        {
            int index = TBCustomizerSettings.FindIndex(s => s.SettingName.ToLower() == Setting.SettingName.ToLower());

            if (index == -1) return;

            TBCustomizerSettings[index] = Setting;
        }

        public Setting FindSetting(string SettingName)
        {
            return TBCustomizerSettings.FirstOrDefault(s => s.SettingName.ToLower().Trim() == SettingName.ToLower().Trim());
        }
    }
}