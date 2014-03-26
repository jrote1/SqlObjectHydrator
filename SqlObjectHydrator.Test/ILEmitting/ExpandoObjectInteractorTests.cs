using System;
using System.Dynamic;
using System.Reflection.Emit;
using NUnit.Framework;
using SqlObjectHydrator.ILEmitting;

namespace SqlObjectHydrator.Test.ILEmitting
{
	[TestFixture]
	public class ExpandoObjectInteractorTests
	{
		[Test]
		public void SetExpandoProperty_WhenValueType_CanSetPropertyValue()
		{
			const int expected = 1;

			var method = new DynamicMethod( "", typeof( object ), new Type[ 0 ], true );
			var emiter = method.GetILGenerator();
			var expando = emiter.DeclareLocal( typeof( ExpandoObject ) );
			emiter.Emit( OpCodes.Newobj, typeof( ExpandoObject ).GetConstructors()[ 0 ] );
			emiter.Emit( OpCodes.Stloc, expando );

			var value = emiter.DeclareLocal( typeof( int ) );
			emiter.Emit( OpCodes.Ldc_I4, expected );
			emiter.Emit( OpCodes.Stloc, value );

			ExpandoObjectInteractor.SetExpandoProperty( emiter, expando, "Test", value );

			emiter.Emit( OpCodes.Ldloc, expando );
			emiter.Emit( OpCodes.Ret );
			var @delegate = (Func<dynamic>)method.CreateDelegate( typeof( Func<dynamic> ) );

			Assert.AreEqual( expected, @delegate().Test );
		}

		[Test]
		public void SetExpandoProperty_WhenNotValueType_CanSetPropertyValue()
		{
			const string expected = "Expected String";

			var method = new DynamicMethod( "", typeof( object ), new Type[ 0 ], true );
			var emiter = method.GetILGenerator();
			var expando = emiter.DeclareLocal( typeof( ExpandoObject ) );
			emiter.Emit( OpCodes.Newobj, typeof( ExpandoObject ).GetConstructors()[ 0 ] );
			emiter.Emit( OpCodes.Stloc, expando );

			var value = emiter.DeclareLocal( typeof( string ) );
			emiter.Emit( OpCodes.Ldstr, expected );
			emiter.Emit( OpCodes.Stloc, value );

			ExpandoObjectInteractor.SetExpandoProperty( emiter, expando, "Test", value );

			emiter.Emit( OpCodes.Ldloc, expando );
			emiter.Emit( OpCodes.Ret );
			var @delegate = (Func<dynamic>)method.CreateDelegate( typeof( Func<dynamic> ) );

			Assert.AreEqual( expected, @delegate().Test );
		}
	}
}