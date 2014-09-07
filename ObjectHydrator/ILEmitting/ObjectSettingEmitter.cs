using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SqlObjectHydrator.ClassMapping;

namespace SqlObjectHydrator.ILEmitting
{
    internal static class ObjectSettingEmitter
    {
        public static List<T> ReadData<T>(IDataReader dataReader, Dictionary<MappingEnum, object> mappings, Dictionary<Type, object> objectSetters, Func<IDataRecord, Type> variableTableTypeFunc = null)
        {
            var result = new List<T>();
            var objectSettersCasted = objectSetters.ToDictionary(x => x.Key, x => (Func<IDataRecord, Dictionary<MappingEnum, object>, T>)x.Value);

            if (variableTableTypeFunc == null)
            {
                Func<IDataRecord, Dictionary<MappingEnum, object>, T> objectSetter;

                if (!objectSettersCasted.ContainsKey(typeof(T)))
                    objectSetters[typeof(T)] = objectSetter = GenerateObjectBuilder<T>(dataReader, mappings);
                else
                    objectSetter = objectSettersCasted[typeof(T)];

                while (dataReader.Read())
                {
                    result.Add(objectSetter(dataReader, mappings));
                }
            }
            else
            {
                while (dataReader.Read())
                {
                    var rowType = variableTableTypeFunc(dataReader);

                    result.Add((T)typeof(ObjectSettingEmitter).GetMethod("VariableType").MakeGenericMethod(typeof(T), rowType).Invoke(null, new object[]{dataReader, mappings, objectSettersCasted}));
                }
            }

            return result;
        }

        public static TBase VariableType<TBase, TActual>(IDataReader dataReader, Dictionary<MappingEnum, object> mappings, Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, TBase>> objectSetters) where TActual : TBase
        {
            if (!objectSetters.ContainsKey(typeof(TActual)))
            {
                var mainFunc = GenerateObjectBuilder<TActual>(dataReader, mappings);
                Func<IDataRecord, Dictionary<MappingEnum, object>, TBase> func = (d, m) => mainFunc(d, m);

                //Fix to use row type
                objectSetters[typeof(TActual)] = func;
                objectSetters = objectSetters.ToDictionary(x => x.Key, x => (Func<IDataRecord, Dictionary<MappingEnum, object>, TBase>)x.Value);
            }

            return objectSetters[typeof(TActual)](dataReader,mappings);
        }

        public static int TableId = 0;

        public static void InitializeDictionaryIfNeeded(Dictionary<int, Dictionary<Type, object>> dictionary, int tableId)
        {
            if (!dictionary.ContainsKey(tableId))
                dictionary.Add(tableId, new Dictionary<Type, object>());
        }

        public static LocalBuilder Emit(ILGenerator emitter, Type type, Mapping mapping, int tableId)
        {
            emitter.Emit(OpCodes.Ldarg_2);
            emitter.Emit(OpCodes.Ldc_I4, tableId);
            emitter.Emit(OpCodes.Call, typeof(ObjectSettingEmitter).GetMethod("InitializeDictionaryIfNeeded"));

            var variableTableTypeFunc = emitter.DeclareLocal(typeof(Func<IDataRecord,Type>));
            emitter.Emit(OpCodes.Ldnull);
            emitter.Emit(OpCodes.Stloc,variableTableTypeFunc);


            if (mapping.VariableTableTypes.ContainsKey(type))
            {
                emitter.Emit(OpCodes.Ldarg_1);
                emitter.Emit(OpCodes.Ldc_I4, (int)MappingEnum.VariableTableType);
                emitter.Emit(OpCodes.Callvirt, typeof(Dictionary<MappingEnum, object>).GetMethod("GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic));
                emitter.Emit(OpCodes.Castclass, typeof(Dictionary<Type, object>));


                emitter.Emit(OpCodes.Ldtoken, type);
                emitter.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                emitter.Emit(OpCodes.Callvirt, typeof(Dictionary<Type, object>).GetProperty("Item").GetGetMethod());
                emitter.Emit(OpCodes.Castclass, typeof(Func<IDataRecord, Type>));
                emitter.Emit(OpCodes.Stloc, variableTableTypeFunc);
            }



            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldarg_1);

            emitter.Emit(OpCodes.Ldarg_2);
            emitter.Emit(OpCodes.Ldc_I4, tableId);
            emitter.Emit(OpCodes.Call, typeof(Dictionary<int, Dictionary<Type,object>>).GetProperty("Item").GetGetMethod());

            emitter.Emit(OpCodes.Ldloc, variableTableTypeFunc);

            emitter.Emit(OpCodes.Call, typeof(ObjectSettingEmitter).GetMethod("ReadData").MakeGenericMethod(type));

            var resultLocalBuilder = emitter.DeclareLocal(typeof(List<>).MakeGenericType(type));
            emitter.Emit(OpCodes.Stloc, resultLocalBuilder);

            return resultLocalBuilder;
        }

        public static Func<IDataRecord, Dictionary<MappingEnum, object>, T> GenerateObjectBuilder<T>(IDataRecord dataRecord, Dictionary<MappingEnum, object> mappings)
        {
            var type = typeof(T);
            var propertyMaps = mappings.ContainsKey(MappingEnum.PropertyMap) ? mappings[MappingEnum.PropertyMap] as Dictionary<PropertyInfo, PropertyMap> : new Dictionary<PropertyInfo, PropertyMap>();
            var filteredPropertyMaps = propertyMaps.Where(x => x.Key.DeclaringType.IsAssignableFrom(type)).ToList();

            var method = new DynamicMethod("", typeof(T), new[]
			{
				typeof( IDataRecord ),
				typeof( Dictionary<MappingEnum, object> )
			}, true);
            var emitter = method.GetILGenerator();


            var localPropertyMapsBuilders = new Dictionary<PropertyInfo, LocalBuilder>();
            if (filteredPropertyMaps.Count > 0)
            {
                var propertyMapsLocal = emitter.DeclareLocal(typeof(List<PropertyMap>));

                emitter.Emit(OpCodes.Ldarg_1);
                emitter.Emit(OpCodes.Ldc_I4, (int)MappingEnum.PropertyMap);
                emitter.Emit(OpCodes.Callvirt, typeof(Dictionary<MappingEnum, object>).GetMethod("GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic));
                emitter.Emit(OpCodes.Castclass, typeof(Dictionary<PropertyInfo, PropertyMap>));
                emitter.Emit(OpCodes.Call, typeof(Dictionary<PropertyInfo, PropertyMap>).GetProperty("Values").GetGetMethod());
                emitter.Emit(OpCodes.Call, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(typeof(PropertyMap)));
                emitter.Emit(OpCodes.Stloc, propertyMapsLocal);

                foreach (var propertyMap in filteredPropertyMaps.Where(x => x.Value.PropertyMapType == PropertyMapType.Func))
                {
                    var localBuilder = emitter.DeclareLocal(propertyMap.Value.SetAction.GetType());
                    var index = propertyMaps.Keys.ToList().IndexOf(propertyMap.Key);

                    emitter.Emit(OpCodes.Ldloc, propertyMapsLocal);
                    emitter.Emit(OpCodes.Ldc_I4, index);
                    emitter.Emit(OpCodes.Call, typeof(List<PropertyMap>).GetProperty("Item").GetGetMethod());
                    emitter.Emit(OpCodes.Call, typeof(PropertyMap).GetProperty("SetAction").GetGetMethod());
                    emitter.Emit(OpCodes.Castclass, propertyMap.Value.SetAction.GetType());
                    emitter.Emit(OpCodes.Stloc, localBuilder);
                    localPropertyMapsBuilders.Add(propertyMap.Key, localBuilder);
                }
            }

            var objectVariable = emitter.DeclareLocal(type);

            emitter.Emit(OpCodes.Newobj, type.GetConstructors()[0]);
            emitter.Emit(OpCodes.Stloc, objectVariable);

            for (var i = 0; i < dataRecord.FieldCount; i++)
            {
                if (type == typeof(ExpandoObject))
                {
                    var valueLocalBuilder = emitter.DeclareLocal(dataRecord.GetFieldType(i));
                    DataReaderEmitter.GetPropertyValue(emitter, dataRecord.GetFieldType(i), i, false);
                    emitter.Emit(OpCodes.Stloc, valueLocalBuilder);
                    ExpandoObjectInteractor.SetExpandoProperty(emitter, objectVariable, dataRecord.GetName(i), valueLocalBuilder);
                }
                else
                {
                    PropertyInfo propertyInfo;
                    if (filteredPropertyMaps.Any(x => x.Value.PropertyMapType == PropertyMapType.ColumnId && x.Value.ColumnId == i))
                        propertyInfo = filteredPropertyMaps.First(x => x.Value.ColumnId == i).Key;
                    else if (filteredPropertyMaps.Any(x => x.Value.PropertyMapType == PropertyMapType.ColumnName && x.Value.ColumnName == dataRecord.GetName(i)))
                        propertyInfo = filteredPropertyMaps.First(x => x.Value.ColumnName == dataRecord.GetName(i)).Key;
                    else if (filteredPropertyMaps.Any(x => x.Key.Name == dataRecord.GetName(i)))
                        propertyInfo = null;
                    else
                        propertyInfo = type.GetProperty(dataRecord.GetName(i));
                    if (propertyInfo != null && propertyInfo.GetActualPropertyType() == dataRecord.GetFieldType(i) && filteredPropertyMaps.Where(x => x.Value.PropertyMapType == PropertyMapType.Func).All(x => x.Key != propertyInfo))
                    {
                        emitter.Emit(OpCodes.Ldloc, objectVariable);
                        DataReaderEmitter.GetPropertyValue(emitter, propertyInfo.PropertyType, i, propertyInfo.IsNullable());
                        emitter.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
                    }
                }
            }

            foreach (var propertyMap in filteredPropertyMaps.Where(x => x.Value.PropertyMapType == PropertyMapType.Func))
            {
                var propertyMapLocal = localPropertyMapsBuilders[propertyMap.Key];

                emitter.Emit(OpCodes.Ldloc, objectVariable);
                emitter.Emit(OpCodes.Ldloc, propertyMapLocal);
                emitter.Emit(OpCodes.Ldarg_0);
                emitter.Emit(OpCodes.Call, propertyMapLocal.LocalType.GetMethod("Invoke"));
                emitter.Emit(OpCodes.Callvirt, propertyMap.Key.GetSetMethod() ?? propertyMap.Key.GetSetMethod(true));
            }

            emitter.Emit(OpCodes.Ldloc, objectVariable);
            emitter.Emit(OpCodes.Ret);

            return (Func<IDataRecord, Dictionary<MappingEnum, object>, T>)method.CreateDelegate(typeof(Func<IDataRecord, Dictionary<MappingEnum, object>, T>));
        }
    }
}