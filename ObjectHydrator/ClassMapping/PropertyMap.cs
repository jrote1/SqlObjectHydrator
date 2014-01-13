using System;

namespace ObjectHydrator.ClassMapping
{
    internal class PropertyMap
    {
        public Boolean Nullable { get; set; }
        public Type Type { get; set; }
        public String Name { get; set; }
        public Int32? FieldId { get; set; }
        public Int32? ConfigurationMapId { get; set; }
    }
}