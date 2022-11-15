using System;
using System.Collections.Generic;
using System.Text;

namespace SqlStringBuilder
{
    public partial class SqlBuilder
    {
        /// <summary>
        /// 创建表模板
        /// </summary>
        public class SqlBuilderCreate : SqlBuilder
        {
            string uniqueColumn = string.Empty;
            string autoIncreaseKey = string.Empty;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="SqlBuilder"></param>
            public SqlBuilderCreate(SqlBuilder SqlBuilder)
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
            /// <returns>create table tableName (col1 type , ... )</returns>
            public new string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("create table ");
                stringBuilder.Append(tableName);
                if (dic.Count > 0)
                {
                    stringBuilder.Append(" (");
                    if (!string.IsNullOrEmpty(autoIncreaseKey))
                    {
                        if (dic.ContainsKey(autoIncreaseKey))
                        {
                            foreach (var pair in dic)
                            {
                                stringBuilder.Append(pair.Key);
                                stringBuilder.Append(" ");
                                stringBuilder.Append(pair.Value);
                                if (pair.Key.Equals(autoIncreaseKey))
                                    stringBuilder.Append(" primary key autoincrement");
                                stringBuilder.Append(",");
                            }
                        }
                        else
                        {
                            stringBuilder.Append("id integer primary key autoincrement,");
                            foreach (var pair in dic)
                            {
                                stringBuilder.Append(pair.Key);
                                stringBuilder.Append(" ");
                                stringBuilder.Append(pair.Value);
                                stringBuilder.Append(",");
                            }
                        }
                    }
                    else
                    {
                        foreach (var pair in dic)
                        {
                            stringBuilder.Append(pair.Key);
                            stringBuilder.Append(" ");
                            stringBuilder.Append(pair.Value);
                            stringBuilder.Append(",");
                        }
                    }
                    if (!string.IsNullOrEmpty(uniqueColumn))
                    {
                        stringBuilder.Append(" unique (");
                        stringBuilder.Append(uniqueColumn);
                        stringBuilder.Append(")");
                    }
                    else
                        stringBuilder.Remove(stringBuilder.Length - 1, 1);
                    stringBuilder.Append(")");
                }
                return stringBuilder.ToString();
            }

            /// <summary>
            /// 声明作用的表
            /// </summary>
            /// <param name="dbtableName"></param>
            /// <returns></returns>
            public new SqlBuilderCreate Table(string dbtableName)
            {
                tableName = dbtableName;
                return this;
            }

            /// <summary>
            /// 列定义（列名，类型定义）
            /// </summary>
            /// <param name="nameTypePairs"></param>
            /// <returns></returns>
            public SqlBuilderCreate ColumnType(params (string, string)[] nameTypePairs)
            {
                foreach (var pair in nameTypePairs)
                {
                    if (dic.ContainsKey(pair.Item1)) continue;
                    dic.Add(pair.Item1, pair.Item2);
                }
                return this;
            }

            /// <summary>
            /// 设置唯一值列（列名）
            /// </summary>
            /// <param name="column"></param>
            /// <returns></returns>
            public SqlBuilderCreate SetUnique(string column)
            {
                uniqueColumn = column;
                return this;
            }

            /// <summary>
            /// 设置自增键
            /// </summary>
            /// <param name="column"></param>
            /// <returns></returns>
            public SqlBuilderCreate SetAutoIncreaseKey(string column)
            {
                autoIncreaseKey = column;
                return this;
            }
        }


    }
}
