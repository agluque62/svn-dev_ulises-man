using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Utilities
{
   public class RegistrySettingsProvider : SettingsProvider
   {
      public RegistrySettingsProvider()
      {

      }

      public override string ApplicationName
      {
         get { return Application.ProductName; }
         set { }
      }

      public override void Initialize(string name, NameValueCollection col)
      {
         base.Initialize(this.ApplicationName, col);
      }

      public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection propvals)
      {
         foreach (SettingsPropertyValue propval in propvals)
         {
            GetRegKey(propval.Property).SetValue(propval.Name, propval.SerializedValue);
         }
      }

      public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection props)
      {
         SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

         foreach (SettingsProperty setting in props)
         {
            SettingsPropertyValue value = new SettingsPropertyValue(setting);
            value.IsDirty = false;
            value.SerializedValue = GetRegKey(setting).GetValue(setting.Name);
            values.Add(value);
         }

         return values;
      }

      // HKLM is used for settings marked as application-scoped.
      // HKLU is used for settings marked as user-scoped.
      private RegistryKey GetRegKey(SettingsProperty prop)
      {
         return Registry.LocalMachine.CreateSubKey(GetSubKeyPath());
      }

      //private bool IsUserScoped(SettingsProperty prop)
      //{
      //   foreach (DictionaryEntry d in prop.Attributes)
      //   {
      //      Attribute a = (Attribute)d.Value;
      //      if (a.GetType() == typeof(UserScopedSettingAttribute))
      //         return true;
      //   }

      //   return false;
      //}

      private string GetSubKeyPath()
      {
         return "Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;
      }
   }
}
