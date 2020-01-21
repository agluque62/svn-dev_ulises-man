using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Globalization;
using System.Security.Permissions;
using System.Configuration.Internal;
using System.Collections;

namespace Utilities
{
   public class LocalFileAppSettingsProvider : LocalFileSettingsProvider
   {
      public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values)
      {
         IDictionary newConnections = new Hashtable();
         IDictionary appSettings = new Hashtable();
         IDictionary localSettings = new Hashtable();

         foreach (SettingsPropertyValue value in values)
         {
            SettingsProperty property = value.Property;

            if (value.IsDirty)
            {
               SpecialSettingAttribute attribute = property.Attributes[typeof(SpecialSettingAttribute)] as SpecialSettingAttribute;

               if ((attribute != null) && (attribute.SpecialSetting == SpecialSetting.ConnectionString))
               {
                  newConnections[property.Name] = value.PropertyValue;
               }
               else
               {
                  StoredSetting setting = new StoredSetting(property.SerializeAs, SerializeToXmlElement(property, value));

                  if (!IsUserSetting(property))
                  {
                     appSettings[property.Name] = setting;
                  }
                  else
                  {
                     localSettings[property.Name] = setting;
                  }
               }

               value.IsDirty = false;
            }
         }

         string sectionName = GetSectionName(context);

         if (newConnections.Count > 0)
         {
            Store.WriteConnectionStrings(newConnections);
         }
         if (localSettings.Count > 0)
         {
            Store.WriteUserSettings(sectionName, localSettings);
         }
         if (appSettings.Count > 0)
         {
            Store.WriteAppSettings(sectionName, appSettings);
         }
      }

      #region Private

      private class XmlEscaper
      {
         private XmlDocument _Doc;
         private XmlElement _Temp;

         public XmlEscaper()
         {
            _Doc = new XmlDocument();
            _Temp = _Doc.CreateElement("temp");
         }

         public string Escape(string xmlString)
         {
            if (string.IsNullOrEmpty(xmlString))
            {
               return xmlString;
            }

            _Temp.InnerText = xmlString;
            return _Temp.InnerXml;
         }

         public string Unescape(string escapedString)
         {
            if (string.IsNullOrEmpty(escapedString))
            {
               return escapedString;
            }

            _Temp.InnerXml = escapedString;
            return _Temp.InnerText;
         }
      }

      private XmlEscaper _Escaper;
      private ClientSettingsStore _Store;

      private XmlEscaper Escaper
      {
         get
         {
            if (_Escaper == null)
            {
               _Escaper = new XmlEscaper();
            }
            return _Escaper;
         }
      }

      private ClientSettingsStore Store
      {
         get
         {
            if (_Store == null)
            {
               _Store = new ClientSettingsStore();
            }
            return _Store;
         }
      }

      private string GetSectionName(SettingsContext context)
      {
         string name = (string)context["GroupName"];
         string key = (string)context["SettingsKey"];

         if (!string.IsNullOrEmpty(key))
         {
            name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", name, key);
         }

         return XmlConvert.EncodeLocalName(name);
      }

      private bool IsUserSetting(SettingsProperty setting)
      {
         bool bUserScoped = setting.Attributes[typeof(UserScopedSettingAttribute)] is UserScopedSettingAttribute;
         bool bAppScoped = setting.Attributes[typeof(ApplicationScopedSettingAttribute)] is ApplicationScopedSettingAttribute;

         if (bUserScoped && bAppScoped)
         {
            throw new ConfigurationErrorsException("BothScopeAttributes");
         }
         if (!bUserScoped && !bAppScoped)
         {
            throw new ConfigurationErrorsException("NoScopeAttributes");
         }

         return bUserScoped;
      }

      private XmlNode SerializeToXmlElement(SettingsProperty setting, SettingsPropertyValue value)
      {
         XmlElement element = new XmlDocument().CreateElement("value");
         string serializedValue = value.SerializedValue as string;

         if ((serializedValue == null) && (setting.SerializeAs == SettingsSerializeAs.Binary))
         {
            byte[] inArray = value.SerializedValue as byte[];
            if (inArray != null)
            {
               serializedValue = Convert.ToBase64String(inArray);
            }
         }

         if (serializedValue == null)
         {
            serializedValue = string.Empty;
         }

         if (setting.SerializeAs == SettingsSerializeAs.String)
         {
            serializedValue = Escaper.Escape(serializedValue);
         }

         element.InnerXml = serializedValue;
         XmlNode oldChild = null;

         foreach (XmlNode node in element.ChildNodes)
         {
            if (node.NodeType == XmlNodeType.XmlDeclaration)
            {
               oldChild = node;
               break;
            }
         }

         if (oldChild != null)
         {
            element.RemoveChild(oldChild);
         }

         return element;
      }

      #endregion
   }

   internal struct StoredSetting
   {
      public SettingsSerializeAs SerializeAs;
      public XmlNode Value;

      public StoredSetting(SettingsSerializeAs serializeAs, XmlNode value)
      {
         SerializeAs = serializeAs;
         Value = value;
      }
   }

   internal sealed class ClientSettingsStore
   {
      public ClientSettingsStore()
      {
      }

      public void WriteUserSettings(string sectionName, IDictionary newSettings)
      {
         Configuration userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
         ClientSettingsSection section = GetUserConfigSection(userConfig, sectionName, true);

         if (section == null)
         {
            throw new ConfigurationErrorsException("SettingsSaveFailedNoSection");
         }

         SettingElementCollection settings = section.Settings;
         foreach (DictionaryEntry entry in newSettings)
         {
            StoredSetting setting = (StoredSetting)entry.Value;
            SettingElement element = settings.Get((string)entry.Key);
            if (element == null)
            {
               element = new SettingElement();
               element.Name = (string)entry.Key;
               settings.Add(element);
            }

            element.SerializeAs = setting.SerializeAs;
            element.Value.ValueXml = setting.Value;
         }
         try
         {
            userConfig.Save();
         }
         catch (ConfigurationErrorsException ex)
         {
            throw new ConfigurationErrorsException("SettingsSaveFailed: " + ex.Message, ex);
         }
      }

      public void WriteAppSettings(string sectionName, IDictionary newSettings)
      {
         Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
         ClientSettingsSection section = GetAppConfigSection(appConfig, sectionName, true);

         if (section == null)
         {
            throw new ConfigurationErrorsException("SettingsSaveFailedNoSection");
         }

         SettingElementCollection settings = section.Settings;
         foreach (DictionaryEntry entry in newSettings)
         {
            StoredSetting setting = (StoredSetting)entry.Value;
            SettingElement element = settings.Get((string)entry.Key);
            if (element == null)
            {
               element = new SettingElement();
               element.Name = (string)entry.Key;
               settings.Add(element);
            }

            element.SerializeAs = setting.SerializeAs;
            element.Value.ValueXml = setting.Value;
         }
         try
         {
            appConfig.Save();
         }
         catch (ConfigurationErrorsException ex)
         {
            throw new ConfigurationErrorsException("SettingsSaveFailed: " + ex.Message, ex);
         }
      }

      public void WriteConnectionStrings(IDictionary newConnections)
      {
         Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
         ConnectionStringsSection section = GetConnectionStringsSection(appConfig, true);

         if (section == null)
         {
            throw new ConfigurationErrorsException("SettingsSaveFailedNoSection");
         }

         ConnectionStringSettingsCollection connections = section.ConnectionStrings;
         foreach (DictionaryEntry entry in newConnections)
         {
            ConnectionStringSettings element = connections[(string)entry.Key];

            if (element == null)
            {
               element = new ConnectionStringSettings();
               element.Name = (string)entry.Key;
               connections.Add(element);
            }

            element.ConnectionString = (string)entry.Value;
         }
         try
         {
            appConfig.Save();
         }
         catch (ConfigurationErrorsException ex)
         {
            throw new ConfigurationErrorsException("SettingsSaveFailed: " + ex.Message, ex);
         }
      }

      #region Private

      private const string ApplicationSettingsGroupName = "applicationSettings";
      private const string ApplicationSettingsGroupPrefix = "applicationSettings/";
      private const string UserSettingsGroupName = "userSettings";
      private const string UserSettingsGroupPrefix = "userSettings/";

      private void DeclareUserSection(Configuration config, string sectionName)
      {
         if (config.GetSectionGroup(UserSettingsGroupName) == null)
         {
            config.SectionGroups.Add(UserSettingsGroupName, new UserSettingsGroup());
         }

         ConfigurationSectionGroup sectionGroup = config.GetSectionGroup(UserSettingsGroupName);
         if ((sectionGroup != null) && (sectionGroup.Sections[sectionName] == null))
         {
            ConfigurationSection section = new ClientSettingsSection();

            section.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
            section.SectionInformation.RequirePermission = false;

            sectionGroup.Sections.Add(sectionName, section);
         }
      }

      private void DeclareAppSection(Configuration config, string sectionName)
      {
         if (config.GetSectionGroup(ApplicationSettingsGroupName) == null)
         {
            config.SectionGroups.Add(ApplicationSettingsGroupName, new ApplicationSettingsGroup());
         }

         ConfigurationSectionGroup sectionGroup = config.GetSectionGroup(ApplicationSettingsGroupName);
         if ((sectionGroup != null) && (sectionGroup.Sections[sectionName] == null))
         {
            ConfigurationSection section = new ClientSettingsSection();

            section.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToApplication;
            section.SectionInformation.RequirePermission = false;

            sectionGroup.Sections.Add(sectionName, section);
         }
      }

      private void DeclareConnectionStringsSection(Configuration config)
      {
         if (config.Sections["connectionStrings"] == null)
         {
            ConfigurationSection section = new ConnectionStringsSection();

            section.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToApplication;
            section.SectionInformation.RequirePermission = false;

            config.Sections.Add("connectionStrings", section);
         }
      }

      private ClientSettingsSection GetUserConfigSection(Configuration config, string sectionName, bool declare)
      {
         string str = UserSettingsGroupPrefix + sectionName;
         ClientSettingsSection section = null;

         if (config != null)
         {
            section = config.GetSection(str) as ClientSettingsSection;
            if ((section == null) && declare)
            {
               DeclareUserSection(config, sectionName);
               section = config.GetSection(str) as ClientSettingsSection;
            }
         }

         return section;
      }

      private ClientSettingsSection GetAppConfigSection(Configuration config, string sectionName, bool declare)
      {
         string str = ApplicationSettingsGroupPrefix + sectionName;
         ClientSettingsSection section = null;

         if (config != null)
         {
            section = config.GetSection(str) as ClientSettingsSection;
            if ((section == null) && declare)
            {
               DeclareAppSection(config, sectionName);
               section = config.GetSection(str) as ClientSettingsSection;
            }
         }

         return section;
      }

      private ConnectionStringsSection GetConnectionStringsSection(Configuration config, bool declare)
      {
         string str = "connectionStrings";
         ConnectionStringsSection section = null;

         if (config != null)
         {
            section = config.GetSection(str) as ConnectionStringsSection;
            if ((section == null) && declare)
            {
               DeclareConnectionStringsSection(config);
               section = config.GetSection(str) as ConnectionStringsSection;
            }
         }

         return section;
      }

      #endregion
   }
}
