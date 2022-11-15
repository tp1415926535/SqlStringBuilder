using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlStringBuilder
{
    public partial class SqlBuilder
    {
        /// <summary>
        /// 添加型模板
        /// </summary>
        public class SqlBuilderInsert : SqlBuilder
        {
            DataTable dataTable;
            string effectColumn;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="SqlBuilder"></param>
            public SqlBuilderInsert(SqlBuilder SqlBuilder)
            {
                tableName = SqlBuilder.tableName;
                //action = SqlBuilder.action;
                list = SqlBuilder.list;
                dic = SqlBuilder.dic;
                whereType = SqlBuilder.whereType;
                whereFilter = SqlBuilder.whereFilter;
                whereColumn = SqlBuilder.whereColumn;
                whereNestedString = SqlBuilder.whereNestedString;
                whereNestedMulty = SqlBuilder.whereNestedMulty;
                whereNoQuoMark = SqlBuilder.whereNoQuoMark;
            }

            /// <summary>
            /// 转为字符串
            /// </summary>
            /// <returns>insert into tableName (col1, ... ) values ('value1', ... )</returns>
            public new string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("insert into ");
                stringBuilder.Append(tableName);
                if (dic.Count > 0)
                {
                    stringBuilder.Append(" (");
                    foreach (var pair in dic)
                    {
                        stringBuilder.Append(pair.Key);
                        stringBuilder.Append(" , ");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 2, 2);
                    stringBuilder.Append(") values ('");
                    foreach (var pair in dic)
                    {
                        stringBuilder.Append(pair.Value);
                        stringBuilder.Append("','");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 2, 2);
                    stringBuilder.Append(") ");
                }
                else if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    stringBuilder.Append(" (");
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        stringBuilder.Append(column.ColumnName);
                        stringBuilder.Append(",");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                    stringBuilder.Append(") values ");
                    foreach (DataRow row in dataTable.Rows)
                    {
                        stringBuilder.Append("('");
                        stringBuilder.Append(string.Join("','", row.ItemArray));//Convert.ToString(row)?
                        stringBuilder.Append("'),");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                }
                else if (list.Count > 0)
                {
                    stringBuilder.Append(" values ('");
                    foreach (string value in list)
                    {
                        stringBuilder.Append(value);
                        stringBuilder.Append("','");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 2, 2);
                    stringBuilder.Append(")");
                }
                if (!string.IsNullOrEmpty(effectColumn))
                {
                    stringBuilder.Append(" returning ");
                    stringBuilder.Append(effectColumn);
                }
                return stringBuilder.ToString();
            }

            /// <summary>
            /// 声明作用的表
            /// </summary>
            /// <param name="dbtableName"></param>
            /// <returns></returns>
            public new SqlBuilderInsert Table(string dbtableName)
            {
                tableName = dbtableName;
                return this;
            }

            /// <summary>
            /// 插入行_省略列（值）
            /// </summary>
            /// <param name="values"></param>
            /// <returns></returns>
            public SqlBuilderInsert ColumnValues(params string[] values)
            {
                foreach (string value in values)
                    list.Add(value);
                return this;
            }

            /// <summary>
            /// 插入行（列名，值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderInsert ColumnValues(params (string, string)[] keyValuePairs)
            {
                foreach (var pair in keyValuePairs)
                {
                    if (dic.ContainsKey(pair.Item1)) continue;
                    dic.Add(pair.Item1, pair.Item2);
                }
                return this;
            }

            /// <summary>
            /// 插入行（字典＜列名，值＞）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderInsert ColumnValues(Dictionary<string, string> keyValuePairs)
            {
                foreach (var pair in keyValuePairs)
                {
                    if (dic.ContainsKey(pair.Key)) continue;
                    dic.Add(pair.Key, pair.Value);
                }
                return this;
            }

            /// <summary>
            /// 插入行（类实例，排除的属性）
            /// </summary>
            /// <param name="customClass"></param>
            /// <param name="exclusion"></param>
            /// <returns></returns>
            public SqlBuilderInsert ColumnValuesFromClass(object customClass, params string[] exclusion)
            {
                //Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                foreach (PropertyInfo property in customClass.GetType().GetProperties())
                {
                    if (exclusion != null && exclusion.Length > 0 && exclusion.Contains(property.Name)) continue;
                    dic[property.Name] = property.GetValue(customClass, null)?.ToString();
                }
                return this;
            }

            /// <summary>
            /// 插入多行（列名，每行值）
            /// </summary>
            /// <param name="columns"></param>
            /// <param name="multyRowPairs"></param>
            /// <returns></returns>
            public SqlBuilderInsert ColumnValues(string[] columns, params string[][] multyRowPairs)
            {
                dataTable = new DataTable();
                foreach (var column in columns)
                    dataTable.Columns.Add(column);
                foreach (var row in multyRowPairs)
                {
                    DataRow dr = dataTable.NewRow();
                    for (int i = 0; i < columns.Length; i++)
                        dr[i] = row[i];
                    dataTable.Rows.Add(dr);
                }
                return this;
            }

            /// <summary>
            /// 插入多行（数据表）
            /// </summary>
            /// <param name="rawDataTable"></param>
            /// <returns></returns>
            public SqlBuilderInsert ColumnValues(DataTable rawDataTable)
            {
                dataTable = rawDataTable.Copy();
                return this;
            }

            /// <summary>
            /// 插入多行（类实例列表，排除的属性）
            /// </summary>
            /// <param name="customClasses"></param>
            /// <param name="exclusion"></param>
            /// <returns></returns>
            public SqlBuilderInsert ColumnValuesFromClasses(IEnumerable<object> customClasses, string[] exclusion = null)
            {
                if (customClasses == null || customClasses.Count() <= 0) return this;

                DataTable dt = new DataTable();
                foreach (PropertyInfo property in customClasses.ElementAt(0).GetType().GetProperties())
                {
                    if (exclusion != null && exclusion.Contains(property.Name)) continue;
                    if (!dt.Columns.Contains(property.Name))
                        dt.Columns.Add(property.Name);
                }
                foreach (var customClass in customClasses)
                {
                    DataRow dr = dt.NewRow();
                    foreach (PropertyInfo property in customClass.GetType().GetProperties())
                    {
                        if (exclusion != null && exclusion.Contains(property.Name)) continue;
                        dr[property.Name] = property.GetValue(customClass, null)?.ToString();
                    }
                    dt.Rows.Add(dr);
                }
                return ColumnValues(dt);
            }

            /// <summary>
            /// 返回影响的行数(依据列名)
            /// </summary>
            /// <param name="column"></param>
            public SqlBuilderInsert ReturnEffectCount(string column = "*")
            {
                effectColumn = column;
                return this;
            }

        }


    }
}
