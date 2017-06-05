
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.ComponentModel;

using Aspectize.Core;

[assembly:AspectizeDALAssemblyAttribute]

namespace TranslationManager
{
	public static partial class SchemaNames
	{
		public static partial class Entities
		{
			public const string AspectizeTranslation = "AspectizeTranslation";
			public const string Language = "Language";
		}
	}

	[SchemaNamespace]
	public class DomainProvider : INamespace
	{
		public string Name { get { return GetType().Namespace; } }
		public static string DomainName { get { return new DomainProvider().Name; } }
	}


	public class TranslationValue : DataWrapper, IDataWrapper, IStructuredData
	{
		void IDataWrapper.InitData(DataRow data, string namePrefix)
		{
			base.InitData(data, namePrefix);
		}

		[Data(IsAccessKey = true)]
		public string Language
		{
			get { return getValue<string>("Language"); }
			set { setValue<string>("Language", value); }
		}

		[Data]
		public string Value
		{
			get { return getValue<string>("Value"); }
			set { setValue<string>("Value", value); }
		}
	}

	[DataDefinition(BeforeUpdate = "AspectizeTriggerService.SetDateNow('DateModified');", BeforeInsert = "AspectizeTriggerService.SetDateNow('DateModified');")]
	public class AspectizeTranslation : Entity, IDataWrapper
	{
		public static partial class Fields
		{
			public const string Id = "Id";
			public const string DateCreated = "DateCreated";
			public const string DateModified = "DateModified";
			public const string Key = "Key";
			public const string Ignore = "Ignore";
			public const string IsNew = "IsNew";
			public const string Values = "Values";
		}

		void IDataWrapper.InitData(DataRow data, string namePrefix)
		{
			base.InitData(data, null);

			Values = new MultiValueField<TranslationValue>(this, buildNamePrefix("Values"));
		}

		[Data(IsPrimaryKey=true)]
		public Guid Id
		{
			get { return getValue<Guid>("Id"); }
			set { setValue<Guid>("Id", value); }
		}

		[Data]
		public DateTime DateCreated
		{
			get { return getValue<DateTime>("DateCreated"); }
			set { setValue<DateTime>("DateCreated", value); }
		}

		[Data]
		public DateTime DateModified
		{
			get { return getValue<DateTime>("DateModified"); }
			set { setValue<DateTime>("DateModified", value); }
		}

		[Data(DefaultValue = "")]
		public string Key
		{
			get { return getValue<string>("Key"); }
			set { setValue<string>("Key", value); }
		}

		[Data(DefaultValue = false)]
		public bool Ignore
		{
			get { return getValue<bool>("Ignore"); }
			set { setValue<bool>("Ignore", value); }
		}

		[Data(DefaultValue = false)]
		public bool IsNew
		{
			get { return getValue<bool>("IsNew"); }
			set { setValue<bool>("IsNew", value); }
		}

		[Data(DefaultValue = "")]
		public MultiValueField<TranslationValue> Values;

	}

	[DataDefinition(MustPersist = false)]
	public class Language : Entity, IDataWrapper
	{
		public static partial class Fields
		{
			public const string Id = "Id";
			public const string Key = "Key";
		}

		void IDataWrapper.InitData(DataRow data, string namePrefix)
		{
			base.InitData(data, null);
		}

		[Data(IsPrimaryKey=true)]
		public Guid Id
		{
			get { return getValue<Guid>("Id"); }
			set { setValue<Guid>("Id", value); }
		}

		[Data]
		public string Key
		{
			get { return getValue<string>("Key"); }
			set { setValue<string>("Key", value); }
		}

	}

}


  
