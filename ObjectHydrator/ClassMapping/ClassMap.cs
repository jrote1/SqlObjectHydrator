using System.Collections.Generic;

namespace SqlObjectHydrator.ClassMapping
{
    internal class ClassMap : PropertyMap
    {
        public List<PropertyMap> Propertys { get; set; }
    }
}