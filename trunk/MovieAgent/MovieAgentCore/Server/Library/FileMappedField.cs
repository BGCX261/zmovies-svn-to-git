using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using System.IO;

namespace MovieAgent.Server.Library
{
	[Script]
	public class FileMappedField
	{
		string InternalValue;

		public event Action ValueChanged;
		public string Value
		{
			get
			{
				return InternalValue;
			}
			set
			{
				InternalValue = value;
				if (ValueChanged != null)
					ValueChanged();
			}
		}

		public override string ToString()
		{
			return InternalValue;
		}

		[Script]
		public class Builder
		{
			public readonly FileInfo Target;

			public Builder(FileInfo Target)
			{
				this.Target = Target;
			}

			public readonly List<FileMappedField> Fields = new List<FileMappedField>();

			public bool IsDirty;

			public static implicit operator FileMappedField(Builder e)
			{
				var n = new FileMappedField();

				e.Fields.Add(n);

				n.ValueChanged +=
					delegate
					{
						e.IsDirty = true;
					};

				return n;
			}

			public bool Trigger;
			public Action this[FileMappedField e]
			{
				set
				{
					if (Trigger)
						return;

					if (!string.IsNullOrEmpty(e.Value))
						return;

					value();

					Trigger = true;
				}
			}

			public FileMappedField this[string e]
			{
				get
				{
					FileMappedField f = this;

					f.Value = e;

					return f;
				}
			}

			public void FromFile()
			{
				using (var r = new StreamReader(this.Target.OpenRead()))
				{
					foreach (var i in this.Fields)
					{
						var x = r.ReadLine();

						if (x == null)
							break;

						i.Value = x;
					}
				}

				this.IsDirty = false;
			}

			public void ToFile()
			{
				using (var w = new StreamWriter(this.Target.OpenWrite()))
				{
					foreach (var i in this.Fields)
					{
						w.WriteLine(i.Value);
					}
				}

				this.IsDirty = false;
			}

			public void ToFileWhenDirty()
			{
				if (this.IsDirty)
					ToFile();
			}
		}

	}

	[Script]
	public static class FileMappedFieldExtensions
	{
		public static FileMappedField.Builder ToFieldBuilder(this FileInfo f)
		{
			return new FileMappedField.Builder(f);
		}
	}
}
