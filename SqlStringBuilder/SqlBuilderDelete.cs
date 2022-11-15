using System;
using System.Collections.Generic;
using System.Text;

namespace SqlStringBuilder
{
    public partial class SqlBuilder
    {        
        /// <summary>
        /// 删除型模板
        /// </summary>
        public class SqlBuilderDelete : SqlBuilder
        {
            string effectColumn;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="SqlBuilder"></param>
            public SqlBuilderDelete(SqlBuilder SqlBuilder)
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
            /// <returns>delete from tabelName [ where xx = 'value' ]</returns>
            public new string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("delete from ");
                stringBuilder.Append(tableName);
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
            public new SqlBuilderDelete Table(string dbtableName)
            {
                tableName = dbtableName;
                return this;
            }

            /// <summary>
            /// 筛选条件（筛选列，筛选值） 默认操作符为 = and
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderDelete Where(params (string, string)[] keyValuePairs)
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
            public SqlBuilderDelete Where(bool noQuotationMark, (string, string)[] keyValuePairs)
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
            public SqlBuilderDelete Where(WhereOperator whereOperator, bool orConnect, bool not, params (string, string)[] keyValuePairs)
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
            public SqlBuilderDelete Where(WhereOperator whereOperator, bool orConnect, bool not, bool noQuotationMark, params (string, string)[] keyValuePairs)
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
            public SqlBuilderDelete WhereIn(string column, bool not = false, params string[] values)
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
            public SqlBuilderDelete WhereBetween(string column, string value1, string value2, bool not = false)
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
            public SqlBuilderDelete Where(string column, WhereOperator whereOperator, string conditionValue, bool not = false)
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
            public SqlBuilderDelete WhereAny(string column, WhereOperator whereOperator, string conditionValue, bool not = false)
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
            public SqlBuilderDelete WhereAll(string column, WhereOperator whereOperator, string conditionValue, bool not = false)
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
            public SqlBuilderDelete WhereIn(string column, string conditionValue, bool not = false)
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
            public SqlBuilderDelete WhereExist(string conditionValue, bool not = false)
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
            public SqlBuilderDelete ReturnEffectCount(string column = "*")
            {
                effectColumn = column;
                return this;
            }
        }

    }
}
