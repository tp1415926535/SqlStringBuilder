using System;
using System.Collections.Generic;
using System.Text;

namespace SqlStringBuilder
{
    public partial class SqlBuilder
    {
        /// <summary>
        /// 读取型模板
        /// </summary>
        public class SqlBuilderRead : SqlBuilder
        {
            bool count = false;
            bool distinct = false;
            Dictionary<string, bool> orderDic = new Dictionary<string, bool>();
            int limit = -1;
            int offset = -1;
            bool percent = false;

            string tarTable;
            string tarDatabase;

            string ViewName;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="SqlBuilder"></param>
            public SqlBuilderRead(SqlBuilder SqlBuilder)
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
            /// <returns>select * from tableName...</returns>
            public new string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(ViewName))
                {
                    stringBuilder.Append("create or replace view '");
                    stringBuilder.Append(ViewName);
                    stringBuilder.Append("' as ");
                }
                stringBuilder.Append("select ");
                if (limit > 0 && percent)
                {
                    stringBuilder.Append(" top ");
                    stringBuilder.Append(limit);
                    stringBuilder.Append(" percent ");
                }
                if (count) stringBuilder.Append(" count( ");
                if (distinct) stringBuilder.Append(" distinct ");
                if (list.Count > 0)
                {
                    foreach (var column in list)
                    {
                        stringBuilder.Append(column);
                        stringBuilder.Append(",");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                }
                if (dic.Count > 0)
                {
                    foreach (var pair in dic)
                    {
                        stringBuilder.Append(pair.Key);
                        stringBuilder.Append(" as ");
                        stringBuilder.Append(pair.Value);
                        stringBuilder.Append(",");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                }
                if (list.Count <= 0 && dic.Count <= 0)
                    stringBuilder.Append(" * ");
                if (count) stringBuilder.Append(")");
                if (!string.IsNullOrEmpty(tarTable))
                {
                    stringBuilder.Append(" into ");
                    stringBuilder.Append(tarTable);
                    if (!string.IsNullOrEmpty(tarDatabase))
                    {
                        stringBuilder.Append(" in '");
                        stringBuilder.Append(tarDatabase);
                        stringBuilder.Append("' ");
                    }
                }
                stringBuilder.Append(" from ");
                stringBuilder.Append(tableName);
                if (whereFilter.Count > 0 || !string.IsNullOrEmpty(whereColumn) || !string.IsNullOrEmpty(whereNestedString))
                    AppendWhereSentence(stringBuilder, this);
                if (orderDic.Count > 0)
                {
                    stringBuilder.Append(" order by ");
                    foreach (var pair in orderDic)
                    {
                        stringBuilder.Append(pair.Key);
                        if (pair.Value == false)
                            stringBuilder.Append(" desc ");
                        stringBuilder.Append(",");
                    }
                    stringBuilder = stringBuilder.Remove(stringBuilder.Length - 1, 1);
                }
                if (limit > 0 && !percent)
                {
                    stringBuilder.Append(" limit ");
                    stringBuilder.Append(limit);
                }
                if (offset > 0)
                {
                    stringBuilder.Append(" offset ");
                    stringBuilder.Append(offset);
                }
                return stringBuilder.ToString();
            }

            /// <summary>
            /// 声明作用的表
            /// </summary>
            /// <param name="dbtableName"></param>
            /// <returns></returns>
            public new SqlBuilderRead Table(string dbtableName)
            {
                tableName = dbtableName;
                return this;
            }

            /// <summary>
            /// 计数
            /// </summary>
            /// <returns></returns>
            public SqlBuilderRead Count()
            {
                count = true;
                return this;
            }

            /// <summary>
            /// 去重
            /// </summary>
            /// <returns></returns>
            public SqlBuilderRead Distinct()
            {
                distinct = true;
                return this;
            }

            /// <summary>
            /// 读取的列名（列名）
            /// </summary>
            /// <param name="columnNames"></param>
            /// <returns></returns>
            public SqlBuilderRead Columns(params string[] columnNames)
            {
                foreach (string value in columnNames)
                    list.Add(value);
                return this;
            }

            /// <summary>
            /// 读取的列名（列名，读作新名）
            /// </summary>
            /// <param name="columnNames"></param>
            /// <returns></returns>
            public SqlBuilderRead ColumnsAs(params (string, string)[] columnNames)
            {
                foreach (var pair in columnNames)
                {
                    if (dic.ContainsKey(pair.Item1)) continue;
                    dic.Add(pair.Item1, pair.Item2);
                }
                return this;
            }


            /// <summary>
            /// 成为复制表语句
            /// </summary>
            /// <param name="targetTable"></param>
            /// <param name="targetDatabase"></param>
            /// <returns>select -> select into </returns>
            public SqlBuilderRead Copy(string targetTable, string targetDatabase = null)
            {
                tarTable = targetTable;
                tarDatabase = targetDatabase;
                return this;
            }

            /// <summary>
            /// 筛选条件（筛选列，筛选值） 默认操作符为 = and
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderRead Where(params (string, string)[] keyValuePairs)
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
            public SqlBuilderRead Where(bool noQuotationMark, params (string, string)[] keyValuePairs)
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
            public SqlBuilderRead Where(WhereOperator whereOperator, bool orConnect, bool not, params (string, string)[] keyValuePairs)
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
            public SqlBuilderRead Where(WhereOperator whereOperator, bool orConnect, bool not, bool noQuotationMark, params (string, string)[] keyValuePairs)
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
            public SqlBuilderRead WhereIn(string column, bool not = false, params string[] values)
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
            public SqlBuilderRead WhereBetween(string column, string value1, string value2, bool not = false)
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
            public SqlBuilderRead Where(string column, WhereOperator whereOperator, string conditionValue, bool not = false)
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
            public SqlBuilderRead WhereAny(string column, WhereOperator whereOperator, string conditionValue, bool not = false)
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
            public SqlBuilderRead WhereAll(string column, WhereOperator whereOperator, string conditionValue, bool not = false)
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
            public SqlBuilderRead WhereIn(string column, string conditionValue, bool not = false)
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
            public SqlBuilderRead WhereExist(string conditionValue, bool not = false)
            {
                whereType = WhereType.Exist;
                whereNot = not;
                whereNestedString = conditionValue;
                return this;
            }


            /// <summary>
            /// 排序（排序列）
            /// </summary>
            /// <param name="orderColumns"></param>
            /// <returns></returns>
            public SqlBuilderRead Order(params string[] orderColumns)
            {
                foreach (var value in orderColumns)
                {
                    if (orderDic.ContainsKey(value)) continue;
                    orderDic.Add(value, true);
                }
                return this;
            }

            /// <summary>
            /// 排序（排序列，是否倒序）
            /// </summary>
            /// <param name="orderColumnsAscend"></param>
            /// <returns></returns>
            public SqlBuilderRead Order(params (string, bool)[] orderColumnsAscend)
            {
                foreach (var pair in orderColumnsAscend)
                {
                    if (orderDic.ContainsKey(pair.Item1)) continue;
                    orderDic.Add(pair.Item1, pair.Item2);
                }
                return this;
            }

            /// <summary>
            /// 限制行数（行数）
            /// </summary>
            /// <param name="count"></param>
            /// <returns></returns>
            public SqlBuilderRead Limit(int count)
            {
                limit = count;
                return this;
            }

            /// <summary>
            /// 跳过行数（行数）
            /// </summary>
            /// <param name="count"></param>
            /// <returns></returns>
            public SqlBuilderRead Offset(int count)
            {
                offset = count;
                return this;
            }

            /// <summary>
            /// 限制行数（数量，是否当做百分比0-100）
            /// </summary>
            /// <param name="count"></param>
            /// <param name="isPercent"></param>
            /// <returns></returns>
            public SqlBuilderRead Top(int count, bool isPercent = false)
            {
                limit = count;
                percent = isPercent;
                return this;
            }

            /// <summary>
            /// 创建视图
            /// </summary>
            /// <param name="viewName"></param>
            /// <returns></returns>
            public SqlBuilderRead AsView(string viewName)
            {
                ViewName = viewName;
                return this;
            }



            /// <summary>
            /// 联合查询sql
            /// </summary>
            /// <param name="sql2"></param>
            /// <param name="distinct"></param>
            /// <returns>sql union sql2</returns>
            public string Union(string sql2, bool distinct = false)
            {
                string connect = distinct ? " union " : " union all ";
                return this.ToString() + connect + sql2;
            }
        }

    }
}
