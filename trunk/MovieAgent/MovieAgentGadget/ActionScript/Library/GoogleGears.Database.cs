using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using ScriptCoreLib.ActionScript.DOM;
using ScriptCoreLib.ActionScript.Extensions;
using ScriptCoreLib.Shared.Lambda;
using MovieAgent.Shared;
//using MovieAgent.Server.Library;

namespace MovieAgentGadget.ActionScript.Library
{
	partial class GoogleGears
	{
		partial class GearsFactory
		{
			Database InternalDatabase;
			public Database Database
			{
				get
				{
					if (InternalDatabase == null)
						InternalDatabase = new Database(this);

					return InternalDatabase;
				}
			}
		}

		[Script]
		public class ResultSet
		{
			public readonly GearsContext Context;
			internal readonly string Token;

			public ResultSet(GearsContext Context, string Token)
			{
				this.Token = Token;
				this.Context = Context;

				this.IsValidRow = () => Context.GoogleGears_ResultSet_isValidRow(Token);
				this.Next = () => Context.Context.ExternalContext_token_call(Token, "next");
				this.Close = () => Context.GoogleGears_ResultSet_close(Token);
				this.FieldCount = () => Context.GoogleGears_ResultSet_fieldCount(Token);
				this.Field =
					index =>
					{
						var FieldToken = Context.Context.CreateToken();

						Context.GoogleGears_ResultSet_field(index, Token, FieldToken);

						return Context.GoogleGears_GetToken(FieldToken);
					};
			}

			/// <summary>
			/// Returns true if you can call data extraction methods.
			/// </summary>
			public readonly Func<bool> IsValidRow;

			/// <summary>
			/// Advances to the next row of the results.
			/// </summary>
			public readonly Action Next;

			/// <summary>
			/// Releases the state associated with this result set. You are required to call close() when you are finished with any result set.
			/// </summary>
			public readonly Action Close;

			/// <summary>
			/// Returns the number of fields in this result set.
			/// </summary>
			public readonly Func<int> FieldCount;

			/// <summary>
			/// Returns the contents of the specified field in the current row.
			/// </summary>
			public readonly Func<int, object> Field;
		}


		// http://code.google.com/apis/gears/api_database.html#Database
		[Script]
		public class Database
		{
			public readonly GearsFactory GearsFactory;

			public event Action<string> BeforeExecute;

			public Database(GearsFactory GearsFactory)
			{
				this.GearsFactory = GearsFactory;

				var DatabaseToken = GearsFactory.Create("beta.database", "1.0");

				this.Open = n => GearsFactory.Context.Context.ExternalContext_token_call_string(DatabaseToken, "open", n);

				this.Execute =
					(sqlStatement, argArray) =>
					{
						var ResultSetToken = GearsFactory.Context.Context.CreateToken();

						if (BeforeExecute != null)
							BeforeExecute(sqlStatement);

						GearsFactory.Context.GoogleGears_Database_execute(sqlStatement, argArray, DatabaseToken, ResultSetToken);

						return new ResultSet(GearsFactory.Context, ResultSetToken);
					};
			}

			/// <summary>
			/// Opens the database name, or an unnamed database if name is omitted. name is 
			/// local to the application's origin (see Security). 
			/// </summary>
			public readonly Action<string> Open;

			/// <summary>
			/// Substitute zero or more bind parameters from argArray into sqlStatement and execute the 
			/// resulting SQL statement. There must be exactly as many items in argArray as 
			/// their are ? placeholders in sqlStatement. argArray can be omitted if 
			/// there are no placeholders. The results of executing the statement are 
			/// returned in a ResultSet.
			/// </summary>
			public readonly ParamsFunc<string, object, ResultSet> Execute;

			public void Create(Type type)
			{
				var f = type.GetFields();

				var a = new StringBuilder();

				a.Append("create table if not exists ");
				a.Append(type.Name);
				a.Append("(");

				var _string = typeof(string);
				var _int = typeof(int);

				for (int i = 0; i < f.Length; i++)
				{
					if (i > 0)
						a.Append(", ");

					var k = f[i];

					a.Append(k.Name);
					a.Append(" ");

					if (k.FieldType.Equals(_string))
						a.Append("varchar(1024)");
					else if (k.FieldType.Equals(_int))
						a.Append("int");
					else
						throw new NotSupportedException(k.FieldType.FullName);

				}

				a.Append(")");

				this.Execute(a.ToString()).Close();

			}

			public void Insert(object data)
			{
				// http://www.w3schools.com/SQL/sql_insert.asp

				var type = data.GetType();
				var f = type.GetFields();

				var a = new StringBuilder();

				a.Append("insert into ");
				a.Append(type.Name);

				a.Append("(");

				for (int i = 0; i < f.Length; i++)
				{
					if (i > 0)
						a.Append(", ");

					a.Append(f[i].Name);
				}
				a.Append(")");

				a.Append(" values ");
				a.Append("(");

				var args = new object[f.Length];

				for (int i = 0; i < f.Length; i++)
				{
					if (i > 0)
						a.Append(", ");

					a.Append("?");

					args[i] = f[i].GetValue(data);
				}

				a.Append(")");

				this.Execute(a.ToString(), args).Close();
			}


			public IEnumerable<T> Fetch<T>(Type type)
				where T: class
			{
				var f = type.GetFields();

				return new DynamicEnumerable<T>
				{
					DynamicGetEnumerator =
						delegate
						{
							var a = new StringBuilder();

							a.Append("select ");

							for (int j = 0; j < f.Length; j++)
							{
								if (j > 0)
									a.Append(", ");

								a.Append(f[j].Name);
							}

							a.Append(" from ");
							a.Append(type.Name);

							var s = this.Execute(a.ToString());

							var Current = default(T);
							var i = 0;

							return new DynamicEnumerator<T>
							{
								DynamicMoveNext =
									delegate
									{
										if (i > 0)
											s.Next();

										Current = (T)Activator.CreateInstance(type);

										var c = s.FieldCount();

										for (int j = 0; j < f.Length; j++)
										{
											if (j < c)
												f[j].SetValue(Current, s.Field(j));
										}

										i++;

										return s.IsValidRow();
									},
								DynamicCurrent =
									delegate
									{
										return Current;
									},
								DynamicDispose =
									delegate
									{
										s.Close();
										s = null;

										Current = null;
									}
							};
						}
				};

			}
		}




	}
}
