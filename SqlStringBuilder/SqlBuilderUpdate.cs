using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlStringBuilder
{
    public partial class SqlBuilder
    {
        /// <summary>
        /// 更新型模板
        /// </summary>
        public class SqlBuilderUpdate : SqlBuilder
        {
            string effectColumn;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="SqlBuilder"></param>
            public SqlBuilderUpdate(SqlBuilder SqlBuilder)
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
            /// <returns>update tableName set col1 = 'value1', ... where xx = 'value'</returns>
            public new string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("update ");
                stringBuilder.Append(tableName);
                if (dic.Count > 0)
                {
                    stringBuilder.Append(" set ");
                    foreach (var pair in dic)
                    {
                        stringBuilder.Append(pair.Key);
                        stringBuilder.Append(" ='");
                        stringBuilder.Append(pair.Value);
                        stringBuilder.Append("',");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                }
                if (whereFilter.Count > 0 || !string.IsNullOrEmpty(whereColumn) || !string.IsNullOrEmpty(whereNestedString))
                    AppendWhereSentence(stringBuilder, this);
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
            public new SqlBuilderUpdate Table(string dbtableName)
            {
                tableName = dbtableName;
                return this;
            }

            /// <summary>
            /// 更新行（列名，值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderUpdate ColumnValues(params (string, string)[] keyValuePairs)
            {
                foreach (var pair in keyValuePairs)
                {
                    if (dic.ContainsKey(pair.Item1)) continue;
                    dic.Add(pair.Item1, pair.Item2);
                }
                return this;
            }

            /// <summary>
            /// 更新行（字典＜列名，值＞）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderUpdate ColumnValues(Dictionary<string, string> keyValuePairs)
            {
                foreach (var pair in keyValuePairs)
                {
                    if (dic.ContainsKey(pair.Key)) continue;
                    dic.Add(pair.Key, pair.Value);
                }
                return this;
            }

            /// <summary>
            /// 更新行（类实例，排除的属性）
            /// </summary>
            /// <param name="customClass"></param>
            /// <param name="exclusion"></param>
            /// <returns></returns>
            public SqlBuilderUpdate ColumnValuesFromClass(object customClass, string[] exclusion = null)
            {
                //Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                foreach (PropertyInfo property in customClass.GetType().GetProperties())
                {
                    if (exclusion != null && exclusion.Contains(property.Name)) continue;
                    dic[property.Name] = property.GetValue(customClass, null)?.ToString();
                }
                return this;
            }


            /// <summary>
            /// 筛选条件（筛选列，筛选值） 默认操作符为 = and
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderUpdate Where(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.Equal;
                whereOrConnect = false;
                whereNot = false;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件（是否不需要给值引号，（筛选列，筛选值）） 默认操作符为 = and
            /// </summary>
            /// <param name="noQuotationMark"></param>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderUpdate Where(bool noQuotationMark, (string, string)[] keyValuePairs)
            {
                whereNoQuoMark = noQuotationMark;
                whereType = WhereType.Equal;
                whereOrConnect = false;
                whereNot = false;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件（操作符，连接符是否为or[默认and]，是否为not，（筛选列，筛选值））
            /// </summary>
            /// <param name="whereOperator"></param>
            /// <param name="orConnect">false and ; true or</param>
            /// <param name="not"></param>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderUpdate Where(WhereOperator whereOperator, bool orConnect, bool not, params (string, string)[] keyValuePairs)
            {
                whereType = (WhereType)whereOperator;
                whereOrConnect = orConnect;
                whereNot = not;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            ///  筛选条件（操作符，连接符是否为or[默认and]，是否为not，是否不需要给值引号，（筛选列，筛选值））
            /// </summary>
            /// <param name="whereOperator"></param>
            /// <param name="orConnect"></param>
            /// <param name="not"></param>
            /// <param name="noQuotationMark"></param>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderUpdate Where(WhereOperator whereOperator, bool orConnect, bool not, bool noQuotationMark, params (string, string)[] keyValuePairs)
            {
                whereNoQuoMark = noQuotationMark;
                whereType = (WhereType)whereOperator;
                whereOrConnect = orConnect;
                whereNot = not;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            private void SetWhereFilter(params (string, string)[] keyValuePairs)
            {
                foreach (var pair in keyValuePairs)
                {
                    if (whereFilter.ContainsKey(pair.Item1)) continue;
                    whereFilter.Add(pair.Item1, pair.Item2);
                }
            }            
            
            /// <summary>
            /// 筛选条件_多个值（筛选列，值）
            /// </summary>
            /// <param name="column"></param>
            /// <param name="not"></param>
            /// <param name="values"></param>
            /// <returns></returns>
            public SqlBuilderUpdate WhereIn(string column, bool not, params string[] values)
            {
                whereType = WhereType.In;
                whereNot = not;
                whereColumn = column;
                foreach (var value in values)
                {
                    if (whereFilter.ContainsKey(value)) continue;
                    whereFilter.Add(value, string.Empty);
                }
                return this;
            }

            /// <summary>
            /// 筛选条件_范围值（筛选列，值边界1，值边界2，是否改取范围外）
            /// </summary>
            /// <param name="column"></param>
            /// <param name="value1"></param>
            /// <param name="value2"></param>
            /// <param name="not"></param>
            /// <returns></returns>
            public SqlBuilderUpdate WhereBetween(string column, string value1, string value2, bool not = false)
            {
                whereType = WhereType.Between;
                whereNot = not;
                whereColumn = column;
                if (!whereFilter.ContainsKey(value1))
                    whereFilter.Add(value1, string.Empty);
                if (!whereFilter.ContainsKey(value2))
                    whereFilter.Add(value2, string.Empty);
                return this;
            }




            /// <summary>
            /// 筛选条件_嵌套语句[单值匹配]（筛选列，操作符，嵌套语句，是否改取范围外）
            /// </summary>
            /// <param name="column"></param>
            /// <param name="whereOperator"></param>
            /// <param name="conditionValue"></param>
            /// <param name="not"></param>
            /// <returns></returns>
            public SqlBuilderUpdate Where(string column, WhereOperator whereOperator, string conditionValue, bool not = false)
            {
                whereType = (WhereType)whereOperator;
                whereNot = not;
                whereColumn = column;
                whereNestedString = conditionValue;
                return this;
            }
            /// <summary>
            /// 筛选条件_嵌套语句[多值匹配其一]（筛选列，操作符，嵌套语句，是否改取范围外）
            /// </summary>
            /// <param name="column"></param>
            /// <param name="whereOperator"></param>
            /// <param name="conditionValue"></param>
            /// <param name="not"></param>
            /// <returns></returns>
            public SqlBuilderUpdate WhereAny(string column, WhereOperator whereOperator, string conditionValue, bool not = false)
            {
                whereType = (WhereType)whereOperator;
                whereNestedMulty = WhereNestedMulty.Any;
                whereNot = not;
                whereColumn = column;
                whereNestedString = conditionValue;
                return this;
            }
            /// <summary>
            /// 筛选条件_嵌套语句[多值匹配全部]（筛选列，操作符，嵌套语句，是否改取范围外）
            /// </summary>
            /// <param name="column"></param>
            /// <param name="whereOperator"></param>
            /// <param name="conditionValue"></param>
            /// <param name="not"></param>
            /// <returns></returns>
            public SqlBuilderUpdate WhereAll(string column, WhereOperator whereOperator, string conditionValue, bool not = false)
            {
                whereType = (WhereType)whereOperator;
                whereNestedMulty = WhereNestedMulty.All;
                whereNot = not;
                whereColumn = column;
                whereNestedString = conditionValue;
                return this;
            }

            /// <summary>
            /// 筛选条件_嵌套语句（筛选列，嵌套语句，是否改取范围外）
            /// </summary>
            /// <param name="column"></param>
            /// <param name="conditionValue"></param>
            /// <param name="not"></param>
            /// <returns></returns>
            public SqlBuilderUpdate WhereIn(string column, string conditionValue, bool not = false)
            {
                whereType = WhereType.In;
                whereNot = not;
                whereColumn = column;
                whereNestedString = conditionValue;
                return this;
            }

            /// <summary>
            /// 筛选条件_嵌套语句（嵌套语句，是否改取范围外）
            /// </summary>
            /// <param name="conditionValue"></param>
            /// <param name="not"></param>
            /// <returns></returns>
            public SqlBuilderUpdate WhereExist(string conditionValue, bool not = false)
            {
                whereType = WhereType.Exist;
                whereNot = not;
                whereNestedString = conditionValue;
                return this;
            }





            /// <summary>
            /// 返回影响的行数(依据列名)
            /// </summary>
            /// <param name="column"></param>
            public SqlBuilderUpdate ReturnEffectCount(string column = "*")
            {
                effectColumn = column;
                return this;
            }
        }

    }
}
