using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBy
{
    public class OrderBySection : ConfigurationSection
    {
        [ConfigurationProperty("classes")]
        public ClassElementCollection Classes { get { return (ClassElementCollection) base["classes"]; } }
    }

    public class ClassElementCollection : ConfigurationElementCollection
    {
        protected override string ElementName
        {
            get { return "class"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        protected override ConfigurationElement CreateNewElement()
        {
            return new ClassElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ClassElement)element).Type;
        }

        public ClassElement this[int index]
        {
            get
            {
                return (ClassElement)BaseGet(index);
            }
        }

        public new IEnumerator<ClassElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i=0; i < count; i++)
                yield return base.BaseGet(i) as ClassElement;
        }
    }

    public class ClassElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"]?.ToString(); }
        }

        [ConfigurationProperty("fields")]
        public FieldElementCollection Fields
        {
            get { return (FieldElementCollection) base["fields"]; }
        }
    }

    public class FieldElementCollection : ConfigurationElementCollection
    {
        protected override string ElementName { get { return "field"; } }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FieldElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FieldElement)element).Name;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        public new IEnumerator<FieldElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
                yield return base.BaseGet(i) as FieldElement;
        }

    }

    public class FieldElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"]?.ToString(); }
        }

        [ConfigurationProperty("orderBy", IsRequired = true)]
        public bool OrderBy
        {
            get
            {
                var s = this["orderBy"]?.ToString().ToUpper() ?? "T";
                return s[0] == 'T';
            }
        }
    }
}
