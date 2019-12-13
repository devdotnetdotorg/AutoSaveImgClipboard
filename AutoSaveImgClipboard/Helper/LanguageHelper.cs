using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace AutoSaveImgClipboard.Helper
{
    public class LanguageChange
    {
        private CultureInfo mCI = null;

        public void ChangeLanguage(string language)
        {
            mCI = new CultureInfo(language);
            ChangeLanguage(mCI);
        }
        public void ChangeLanguage(CultureInfo mCI)
        {
            Thread.CurrentThread.CurrentCulture = mCI;
            Thread.CurrentThread.CurrentUICulture = mCI;
        }

        public void ApplyLanguageToForm(Form inputForm)
        {
            Form mainForm = inputForm;
            while (mainForm.Owner != null)
            {
                mainForm = mainForm.Owner;
            }
            ApplyLanguage(mainForm, null);
        }

        private void ApplyLanguage(object value, ComponentResourceManager resources)
        {
            if (value is Form)
            {
                resources = new ComponentResourceManager(value.GetType());
                resources.ApplyResources(value, "$this");
            }
            Type type = value.GetType();

            foreach (PropertyInfo info in type.GetProperties())
            {
                switch (info.Name)
                {
                    case "Items":
                    case "DropDownItems":
                    case "Controls":
                    case "OwnedForms":

                        if (info.PropertyType.GetInterface("IEnumerable") != null)
                        {
                            IEnumerable collection = (IEnumerable)info.GetValue(value, null);
                            if (collection != null)
                            {
                                foreach (object o in collection)
                                {
                                    PropertyInfo objNameProp = o.GetType().GetProperty("Name");
                                    ApplyResourceOnType(resources, o, objNameProp);
                                }

                            }
                        }
                        break;
                    case "ContextMenuStrip":
                        object obj = (object)info.GetValue(value, null);
                        if (obj != null)
                        {
                            ApplyLanguage(obj, resources);
                            resources.ApplyResources(obj, info.Name, mCI);
                        }
                        break;
                    default:
                        break;
                }
            }

        }

        private void ApplyResourceOnType(ComponentResourceManager resources, object o, PropertyInfo objNameProp)
        {
            switch (o.GetType().Name)
            {
                case "ComboBox":
                    for (int i = 0; i < ((ComboBox)o).Items.Count; i++)
                    {
                        ((ComboBox)o).Items[i] = resources.GetString(GetItemName(o, objNameProp, i), mCI);
                    }
                    break;
                case "ListBox":
                    for (int i = 0; i < ((ListBox)o).Items.Count; i++)
                    {
                        ((ListBox)o).Items[i] = resources.GetString(GetItemName(o, objNameProp, i), mCI);
                    }
                    break;
                // Other classes with string items
                default:
                    if (objNameProp != null)
                    {
                        string name = objNameProp.GetValue(o, null).ToString();
                        ApplyLanguage(o, resources);
                        resources.ApplyResources(o, name, mCI);
                    }
                    break;
            }
        }

        private string GetItemName(object o, PropertyInfo objNameProp, int i)
        {
            string name = String.Format("{0}.{1}",
                objNameProp.GetValue(o, null).ToString(),
                "Items");
            if (i != 0) name = String.Format("{0}{1}", name, i);
            return name;
        }
    }
}
