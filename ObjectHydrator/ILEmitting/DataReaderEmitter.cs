using System;
using System.Data;
using System.Linq;
using System.Reflection.Emit;

namespace SqlObjectHydrator.ILEmitting
{
	internal static class DataReaderEmitter
	{
		public static void GetPropertyValue( ILGenerator emitter, Type type, int fieldId, bool nullable, object map = null )
		{
			if ( nullable )
			{
				var tempValue = emitter.DeclareLocal( type );

				var ifNotNull = emitter.DefineLabel();
				emitter.Emit( OpCodes.Ldarg_0 );
				emitter.Emit( OpCodes.Ldc_I4, fieldId );
				emitter.Emit( OpCodes.Callvirt, typeof( IDataRecord ).GetMethod( "GetValue" ) );
				emitter.Emit( OpCodes.Ldsfld, typeof( DBNull ).GetField( "Value" ) );
				emitter.Emit( OpCodes.Ceq );
				emitter.Emit( OpCodes.Brtrue, ifNotNull );

				emitter.Emit( OpCodes.Ldarg_0 );
				emitter.Emit( OpCodes.Ldc_I4, fieldId );
				emitter.Emit( OpCodes.Callvirt, typeof( IDataRecord ).GetMethod( GetDataReaderMethodName( type.GetGenericArguments()[ 0 ] ) ) );
				emitter.Emit( OpCodes.Newobj, type.GetConstructors().First( x => x.GetParameters().First().ParameterType == type.GetGenericArguments()[ 0 ] ) );
				emitter.Emit( OpCodes.Stloc, tempValue );
				emitter.MarkLabel( ifNotNull );

				emitter.Emit( OpCodes.Ldloc, tempValue );
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
					emitter.Emit( OpCodes.Callvirt, typeof( IDataRecord ).GetMethod( GetDataReaderMethodName( type ) ) );
			}
		}

		private static string GetDataReaderMethodName( Type propertyType )
		{
			if ( propertyType == typeof( Boolean ) )
				return "GetBoolean";
			if ( propertyType == typeof( Byte ) )
				return "GetByte";
			if ( propertyType == typeof( Char ) )
				return "GetChar";
			if ( propertyType == typeof( DateTime ) )
				return "GetDateTime";
			if ( propertyType == typeof( Decimal ) )
				return "GetDecimal";
			if ( propertyType == typeof( Double ) )
				return "GetDouble";
			if ( propertyType == typeof( Single ) )
				return "GetFloat";
			if ( propertyType == typeof( Guid ) )
				return "GetGuid";
			if ( propertyType == typeof( Int16 ) )
				return "GetInt16";
			if ( propertyType == typeof( Int32 ) )
				return "GetInt32";
			if ( propertyType == typeof( Int64 ) )
				return "GetInt64";
			return "GetString";
		}
	}
}