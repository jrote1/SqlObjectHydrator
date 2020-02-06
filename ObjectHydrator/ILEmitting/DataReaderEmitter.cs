using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SqlObjectHydrator.ILEmitting
{
	internal static class DataReaderEmitter
	{
		public static void GetPropertyValue(
		  ILGenerator emitter,
		  Type type,
		  int fieldId,
		  bool nullable,
		  object map = null )
		{
			if ( nullable )
			{
				LocalBuilder local = emitter.DeclareLocal( type );
				emitter.Emit( OpCodes.Ldloca_S, local );
				emitter.Emit( OpCodes.Initobj, type );
				Label label = emitter.DefineLabel();
				emitter.Emit( OpCodes.Ldarg_0 );
				emitter.Emit( OpCodes.Ldc_I4, fieldId );
				emitter.Emit( OpCodes.Callvirt, typeof( IDataRecord ).GetMethod( "IsDBNull" ) );
				emitter.Emit( OpCodes.Brtrue, label );
				emitter.Emit( OpCodes.Ldarg_0 );
				emitter.Emit( OpCodes.Ldc_I4, fieldId );
				emitter.Emit( OpCodes.Callvirt, typeof( IDataRecord ).GetMethod( DataReaderEmitter.GetDataReaderMethodName( type.GetGenericArguments()[ 0 ] ) ) );
				emitter.Emit( OpCodes.Newobj, ( (IEnumerable<ConstructorInfo>)type.GetConstructors() ).First<ConstructorInfo>( (Func<ConstructorInfo, bool>)( x => ( (IEnumerable<ParameterInfo>)x.GetParameters() ).First<ParameterInfo>().ParameterType == type.GetGenericArguments()[ 0 ] ) ) );
				emitter.Emit( OpCodes.Stloc, local );
				emitter.MarkLabel( label );
				emitter.Emit( OpCodes.Ldloc, local );
			}
			else
			{
				emitter.Emit( OpCodes.Ldarg_0 );
				emitter.Emit( OpCodes.Ldc_I4, fieldId );
				if ( type == typeof( string ) )
				{
					emitter.Emit( OpCodes.Callvirt, typeof( IDataRecord ).GetMethod( "GetValue" ) );
					emitter.Emit( OpCodes.Isinst, typeof( string ) );
				}
				else
					emitter.Emit( OpCodes.Callvirt, typeof( IDataRecord ).GetMethod( DataReaderEmitter.GetDataReaderMethodName( type ) ) );
			}
		}

		private static string GetDataReaderMethodName( Type propertyType )
		{
			if ( propertyType == typeof( bool ) )
				return "GetBoolean";
			if ( propertyType == typeof( byte ) )
				return "GetByte";
			if ( propertyType == typeof( char ) )
				return "GetChar";
			if ( propertyType == typeof( DateTime ) )
				return "GetDateTime";
			if ( propertyType == typeof( Decimal ) )
				return "GetDecimal";
			if ( propertyType == typeof( double ) )
				return "GetDouble";
			if ( propertyType == typeof( float ) )
				return "GetFloat";
			if ( propertyType == typeof( Guid ) )
				return "GetGuid";
			if ( propertyType == typeof( short ) )
				return "GetInt16";
			if ( propertyType == typeof( int ) )
				return "GetInt32";
			return propertyType == typeof( long ) ? "GetInt64" : "GetString";
		}
	}
}
