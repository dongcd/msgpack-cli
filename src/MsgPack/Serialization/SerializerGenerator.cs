﻿#region -- License Terms --
//
// MessagePack for CLI
//
// Copyright (C) 2010-2014 FUJIWARA, Yusuke
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
#endregion -- License Terms --

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using MsgPack.Serialization.AbstractSerializers;
using MsgPack.Serialization.CodeDomSerializers;
using MsgPack.Serialization.EmittingSerializers;

namespace MsgPack.Serialization
{
	/// <summary>
	///		Provides pre-compiled serialier assembly generation.
	/// </summary>
	/// <remarks>
	///		Currently, generated assembly has some restrictions:
	///		<list type="bullet">
	///			<item>
	///				The type name cannot be customize. It always to be <c>MsgPack.Serialization.EmittingSerializers.Generated.&lt;ESCAPED_TARGET_NAME&gt;Serializer</c>.
	///				Note that the <c>ESCAPED_TARGET_NAME</c> is the string generated by replacing type delimiters with undersecore ('_'). 
	///			</item>
	///			<item>
	///				The assembly cannot be used on WinRT because 
	///			</item>
	///		</list>
	///		<note>
	///			You should <strong>NOT</strong> assume at all like class hierarchy of generated type, its implementing interfaces, custom attributes, or dependencies.
	///			They subject to be changed in the future.
	///			If you want to get such fine grained control for them, you should implement own hand made serializers.
	///		</note>
	/// </remarks>
	public class SerializerGenerator
	{
		/// <summary>
		///		Gets the type of the root object which will be serialized/deserialized.
		/// </summary>
		/// <value>
		///		The first entry of <see cref="TargetTypes"/>.
		///		This value will be <c>null</c> when the <see cref="TargetTypes"/> is empty.
		/// </value>
		[Obsolete( "Use TargetTypes instead." )]
		public Type RootType
		{
			get { return this._targetTypes.FirstOrDefault(); }
		}

		private readonly HashSet<Type> _targetTypes;

		/// <summary>
		///		Gets target types will be generated dedicated serializers.
		/// </summary>
		/// <value>
		///		A collection which stores target types will be generated dedicated serializers.
		/// </value>
		[Obsolete( "Use static methods instead." )]
		public ICollection<Type> TargetTypes
		{
			get { return this._targetTypes; }
		}

		private readonly AssemblyName _assemblyName;

		/// <summary>
		///		Gets the name of the assembly to be generated.
		/// </summary>
		/// <value>
		///		The name of the assembly to be generated.
		/// </value>
		[Obsolete( "Use static methods instead." )]
		public AssemblyName AssemblyName
		{
			get { return this._assemblyName; }
		}

		private SerializationMethod _method;

		/// <summary>
		///		Gets or sets the <see cref="SerializationMethod"/> which indicates serialization method to be used.
		/// </summary>
		/// <value>
		///		The <see cref="SerializationMethod"/> which indicates serialization method to be used.
		/// </value>
		[Obsolete( "Use static methods instead." )]
		public SerializationMethod Method
		{
			get { return this._method; }
			set
			{

				if ( value != SerializationMethod.Array && value != SerializationMethod.Map )
				{
					throw new ArgumentOutOfRangeException( "value" );
				}
				this._method = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializerGenerator"/> class.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to be generated.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="assemblyName"/> is <c>null</c>.
		/// </exception>
		[Obsolete( "Use static methods instead." )]
		public SerializerGenerator( AssemblyName assemblyName )
		{
			if ( assemblyName == null )
			{
				throw new ArgumentNullException( "assemblyName" );
			}

			Contract.EndContractBlock();


			this._assemblyName = assemblyName;
			this._targetTypes = new HashSet<Type>();
			this._method = SerializationMethod.Array;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializerGenerator"/> class.
		/// </summary>
		/// <param name="rootType">Type of the root object which will be serialized/deserialized.</param>
		/// <param name="assemblyName">Name of the assembly to be generated.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="rootType"/> is <c>null</c>.
		///		Or <paramref name="assemblyName"/> is <c>null</c>.
		/// </exception>
		[Obsolete( "Use static methods instead." )]
		public SerializerGenerator( Type rootType, AssemblyName assemblyName )
			: this( assemblyName )
		{
			if ( rootType == null )
			{
				throw new ArgumentNullException( "rootType" );
			}

			Contract.EndContractBlock();


			this._targetTypes.Add( rootType );
		}

		/// <summary>
		///		Generates the serializer assembly and save it to current directory.
		///	</summary>
		/// <returns>The path of generated files.</returns>
		/// <exception cref="IOException">Some I/O error is occurred on saving assembly file.</exception>
		[Obsolete( "Use static GenerateAssembly method instead." )]
		public void GenerateAssemblyFile()
		{
			this.GenerateAssemblyFile( Path.GetFullPath( "." ) );
		}

		/// <summary>
		///		Generates the serializer assembly and save it to specified directory.
		///	</summary>
		/// <param name="directory">The path of directory where newly generated assembly file will be located. If the directory does not exist, then it will be created.</param>
		/// <returns>The path of generated files.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="directory"/> is not valid.</exception>
		/// <exception cref="PathTooLongException"><paramref name="directory"/> is too long.</exception>
		/// <exception cref="DirectoryNotFoundException"><paramref name="directory"/> is existent file.</exception>
		/// <exception cref="UnauthorizedAccessException">Cannot create specified directory for access control of file system.</exception>
		/// <exception cref="IOException">Some I/O error is occurred on creating directory or saving assembly file.</exception>
		[Obsolete( "Use static GenerateAssembly method instead." )]
		public void GenerateAssemblyFile( string directory )
		{
			if ( !Directory.Exists( directory ) )
			{
				Directory.CreateDirectory( directory );
			}

			GenerateAssembly(
				new SerializerAssemblyGenerationConfiguration
				{
					AssemblyName = this._assemblyName,
					OutputDirectory = directory,
					SerializationMethod = this._method
				},
				this._targetTypes
			);
		}

		/// <summary>
		///		Generates an assembly which contains auto-generated serializer types for specified types.
		/// </summary>
		/// <param name="configuration">The <see cref="SerializerAssemblyGenerationConfiguration"/> which holds required <see cref="SerializerAssemblyGenerationConfiguration.AssemblyName"/> and optional settings.</param>
		/// <param name="targetTypes">The target types where serializer types to be generated.</param>
		/// <returns>The file path for generated single module assembly file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <c>null</c>. Or, <paramref name="targetTypes"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="SerializerAssemblyGenerationConfiguration.AssemblyName"/> of <paramref name="configuration"/> is not set correctly.</exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">Failed to generate a serializer because of <paramref name="targetTypes"/>.</exception>
		/// <remarks>
		///		Serializer types for dependent types which are refered from specified <paramref name="targetTypes"/> are automatically generated.
		/// </remarks>
		public static string GenerateAssembly( SerializerAssemblyGenerationConfiguration configuration, params Type[] targetTypes )
		{
			return GenerateAssembly( configuration, targetTypes as IEnumerable<Type> );
		}

		/// <summary>
		///		Generates an assembly which contains auto-generated serializer types for specified types.
		/// </summary>
		/// <param name="configuration">The <see cref="SerializerAssemblyGenerationConfiguration"/> which holds required <see cref="SerializerAssemblyGenerationConfiguration.AssemblyName"/> and optional settings.</param>
		/// <param name="targetTypes">The target types where serializer types to be generated.</param>
		/// <returns>The file path for generated single module assembly file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <c>null</c>. Or, <paramref name="targetTypes"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="SerializerAssemblyGenerationConfiguration.AssemblyName"/> of <paramref name="configuration"/> is not set correctly.</exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">Failed to generate a serializer because of <paramref name="targetTypes"/>.</exception>
		/// <remarks>
		///		Serializer types for dependent types which are refered from specified <paramref name="targetTypes"/> are automatically generated.
		/// </remarks>
		public static string GenerateAssembly( SerializerAssemblyGenerationConfiguration configuration, IEnumerable<Type> targetTypes )
		{
			return
				Path.GetFullPath(
					Path.Combine(
						configuration == null ? "." : configuration.OutputDirectory,
						new SerializerAssemblyGenerationLogic().Generate( targetTypes, configuration ).Single()
					)
				);
		}

		/// <summary>
		///		Generates source codes which implement auto-generated serializer types for specified types with default configuration.
		/// </summary>
		/// <param name="targetTypes">The target types where serializer types to be generated.</param>
		/// <returns>The file path for generated single module assembly file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="targetTypes"/> is <c>null</c>.</exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">Failed to generate a serializer because of <paramref name="targetTypes"/>.</exception>
		/// <remarks>
		///		Serializer types for dependent types which are refered from specified <paramref name="targetTypes"/> are NOT generated.
		///		This method just generate serializer types for specified types.
		/// </remarks>
		public static IEnumerable<string> GenerateCode( params Type[] targetTypes )
		{
			return GenerateCode( null, targetTypes as IEnumerable<Type> );
		}

		/// <summary>
		///		Generates source codes which implement auto-generated serializer types for specified types with default configuration.
		/// </summary>
		/// <param name="targetTypes">The target types where serializer types to be generated.</param>
		/// <returns>The file path for generated single module assembly file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="targetTypes"/> is <c>null</c>.</exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">Failed to generate a serializer because of <paramref name="targetTypes"/>.</exception>
		/// <remarks>
		///		Serializer types for dependent types which are refered from specified <paramref name="targetTypes"/> are NOT generated.
		///		This method just generate serializer types for specified types.
		/// </remarks>
		public static IEnumerable<string> GenerateCode( IEnumerable<Type> targetTypes )
		{
			return GenerateCode( null, targetTypes );
		}

		/// <summary>
		///		Generates source codes which implement auto-generated serializer types for specified types with specified configuration.
		/// </summary>
		/// <param name="configuration">The <see cref="SerializerCodeGenerationConfiguration"/> which holds optional settings. Specifying <c>null</c> means using default settings.</param>
		/// <param name="targetTypes">The target types where serializer types to be generated.</param>
		/// <returns>The file path for generated single module assembly file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="targetTypes"/> is <c>null</c>.</exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">Failed to generate a serializer because of <paramref name="targetTypes"/>.</exception>
		/// <remarks>
		///		Serializer types for dependent types which are refered from specified <paramref name="targetTypes"/> are NOT generated.
		///		This method just generate serializer types for specified types.
		/// </remarks>
		public static IEnumerable<string> GenerateCode( SerializerCodeGenerationConfiguration configuration, params Type[] targetTypes )
		{
			return GenerateCode( configuration, targetTypes as IEnumerable<Type> );
		}

		/// <summary>
		///		Generates source codes which implement auto-generated serializer types for specified types with specified configuration.
		/// </summary>
		/// <param name="configuration">The <see cref="SerializerCodeGenerationConfiguration"/> which holds optional settings. Specifying <c>null</c> means using default settings.</param>
		/// <param name="targetTypes">The target types where serializer types to be generated.</param>
		/// <returns>The file path for generated single module assembly file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="targetTypes"/> is <c>null</c>.</exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException">Failed to generate a serializer because of <paramref name="targetTypes"/>.</exception>
		/// <remarks>
		///		Serializer types for dependent types which are refered from specified <paramref name="targetTypes"/> are NOT generated.
		///		This method just generate serializer types for specified types.
		/// </remarks>
		public static IEnumerable<string> GenerateCode( SerializerCodeGenerationConfiguration configuration, IEnumerable<Type> targetTypes )
		{
			return new SerializerCodesGenerationLogic().Generate( targetTypes, configuration ?? new SerializerCodeGenerationConfiguration() );
		}

		private abstract class SerializerGenerationLogic<TConfig>
			where TConfig : class, ISerializerGeneratorConfiguration
		{
			protected abstract EmitterFlavor EmitterFlavor { get; }

			public IEnumerable<string> Generate( IEnumerable<Type> targetTypes, TConfig configuration )
			{
				if ( targetTypes == null )
				{
					throw new ArgumentNullException( "targetTypes" );
				}

				if ( configuration == null )
				{
					throw new ArgumentNullException( "configuration" );
				}

				configuration.Validate();
				var context =
					new SerializationContext
					{
						EmitterFlavor = this.EmitterFlavor,
						GeneratorOption = SerializationMethodGeneratorOption.CanDump,
						SerializationMethod = configuration.SerializationMethod
					};

				var generationContext = this.CreateGenerationContext( context, configuration );
				var generatorFactory = this.CreateGeneratorFactory();
				foreach ( var targetType in targetTypes.Distinct() )
				{
					var generator = generatorFactory( targetType );
					generator.BuildSerializerCode( generationContext );
				}

				Directory.CreateDirectory( configuration.OutputDirectory );

				return generationContext.Generate();
			}

			protected abstract ISerializerCodeGenerationContext CreateGenerationContext( SerializationContext context, TConfig configuration );

			protected abstract Func<Type, ISerializerCodeGenerator> CreateGeneratorFactory();
		}

		private sealed class SerializerAssemblyGenerationLogic : SerializerGenerationLogic<SerializerAssemblyGenerationConfiguration>
		{
			protected override EmitterFlavor EmitterFlavor
			{
				get { return EmitterFlavor.FieldBased; }
			}

			public SerializerAssemblyGenerationLogic() { }

			protected override ISerializerCodeGenerationContext CreateGenerationContext( SerializationContext context, SerializerAssemblyGenerationConfiguration configuration )
			{
				return
					new AssemblyBuilderCodeGenerationContext(
						context,
						AppDomain.CurrentDomain.DefineDynamicAssembly( configuration.AssemblyName, AssemblyBuilderAccess.RunAndSave, configuration.OutputDirectory )
					);
			}

			protected override Func<Type, ISerializerCodeGenerator> CreateGeneratorFactory()
			{
				return
					type =>
					Activator.CreateInstance(
						typeof( AssemblyBuilderSerializerBuilder<> ).MakeGenericType( type )
					) as ISerializerCodeGenerator;
			}
		}

		private sealed class SerializerCodesGenerationLogic : SerializerGenerationLogic<SerializerCodeGenerationConfiguration>
		{
			protected override EmitterFlavor EmitterFlavor
			{
				get { return EmitterFlavor.CodeDomBased; }
			}

			public SerializerCodesGenerationLogic() { }

			protected override ISerializerCodeGenerationContext CreateGenerationContext( SerializationContext context, SerializerCodeGenerationConfiguration configuration )
			{
				return new CodeDomContext( context, configuration );
			}

			protected override Func<Type, ISerializerCodeGenerator> CreateGeneratorFactory()
			{
				return
					type =>
					Activator.CreateInstance(
						typeof( CodeDomSerializerBuilder<> ).MakeGenericType( type )
					) as ISerializerCodeGenerator;
			}
		}
	}
}
