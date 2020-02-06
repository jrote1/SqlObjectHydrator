using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;

namespace SqlObjectHydrator.ILEmitting
{
	internal static class ExpandoObjectInteractor
	{
		public static void SetExpandoProperty(
		  ILGenerator emiter,
		  LocalBuilder expando,
		  string propertyName,
		  LocalBuilder value )
		{
			emiter.Emit( OpCodes.Ldloc, expando );
			emiter.Emit( OpCodes.Ldnull );
			emiter.Emit( OpCodes.Ldc_I4, -1 );
			emiter.Emit( OpCodes.Ldloc, value );
			if ( value.LocalType.IsValueType )
				emiter.Emit( OpCodes.Box, value.LocalType );
			else
				emiter.Emit( OpCodes.Castclass, typeof( object ) );
			emiter.Emit( OpCodes.Ldstr, propertyName );
			emiter.Emit( OpCodes.Ldc_I4_0 );
			emiter.Emit( OpCodes.Ldc_I4_1 );
			emiter.Emit( OpCodes.Call, typeof( ExpandoObject ).GetMethod( "TrySetValue", BindingFlags.Instance | BindingFlags.NonPublic ) );
		}
	}
}
