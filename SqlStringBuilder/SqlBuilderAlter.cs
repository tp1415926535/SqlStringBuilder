using System;
using System.Collections.Generic;
using System.Text;

namespace SqlStringBuilder
{
    public partial class SqlBuilder
    {        
        /// <summary>
        /// 修改表模板
        /// </summary>
        public class SqlBuilderAlter : SqlBuilder
        {
            bool addColumn = true;
            TargetType targetType = TargetType.None;
            string target = string.Empty;
            string newName = string.Empty;
            string newType = string.Empty;
            enum TargetType
            {
                None,
                ChangeColumn,
                RenameTable,
                SetUnique,
                DropUnique,
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="SqlBuilder"></param>
            public SqlBuilderAlter(SqlBuilder SqlBuilder)
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
            /// <returns>alter table tableName...</returns>
            public new string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("alter table ");
                if (dic.Count > 0)
                {
                    if (addColumn)
                    {
                        stringBuilder.Append(" add ");
                        stringBuilder.Append(" (");
                        foreach (var pair in dic)
                        {
                            stringBuilder.Append(pair.Key);
                            stringBuilder.Append(" ");
                            stringBuilder.Append(pair.Value);
                            stringBuilder.Append(",");
                        }
                        stringBuilder.Remove(stringBuilder.Length - 1, 1);
                        stringBuilder.Append(")");
                    }
                    else
                    {
                        stringBuilder.Append(" alert column ");
                        stringBuilder.Append(" (");
                        foreach (var pair in dic)
                        {
                            stringBuilder.Append(pair.Key);
                            stringBuilder.Append(" ");
                            stringBuilder.Append(pair.Value);
                            stringBuilder.Append(",");
                        }
                        stringBuilder.Remove(stringBuilder.Length - 1, 1);
                        stringBuilder.Append(")");
                    }
                }
                else if (list.Count > 0)
                {
                    stringBuilder.Append(" drop column ");
                    foreach (var column in list)
                    {
                        stringBuilder.Append(column);
                        stringBuilder.Append(",");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                }
                else
                {
                    switch (targetType)
                    {
                        case TargetType.ChangeColumn:
                            stringBuilder.Append(" change column ");
                            stringBuilder.Append(target);
                            stringBuilder.Append(" ");
                            stringBuilder.Append(newName);
                            stringBuilder.Append(" ");
                            stringBuilder.Append(newType);
                            break;
                        case TargetType.RenameTable:
                            stringBuilder.Append(" rename to ");
                            stringBuilder.Append(target);
                            break;
                        case TargetType.SetUnique:
                            stringBuilder.Append(" add unique(");
                            stringBuilder.Append(target);
                            stringBuilder.Append(")");
                            break;
                        case TargetType.DropUnique:
                            stringBuilder.Append(" drop index ");
                            stringBuilder.Append(target);
                            break;
                    }
                }
                return stringBuilder.ToString();
            }

            /// <summary>
            /// 声明作用的表
            /// </summary>
            /// <param name="dbtableName"></param>
            /// <returns></returns>
            public new SqlBuilderAlter Table(string dbtableName)
            {
                tableName = dbtableName;
                return this;
            }

            /// <summary>
            /// 添加列（列名，类型定义）
            /// </summary>
            /// <param name="nameTypePairs"></param>
            /// <returns></returns>
            public SqlBuilderAlter AddColumn(params (string, string)[] nameTypePairs)
            {
                addColumn = true;
                foreach (var pair in nameTypePairs)
                {
                    if (dic.ContainsKey(pair.Item1)) continue;
                    dic.Add(pair.Item1, pair.Item2);
                }
                return this;
            }

            /// <summary>
            /// 删除列（列名）
            /// </summary>
            /// <param name="columns"></param>
            /// <returns></returns>
            public SqlBuilderAlter DropColumn(params string[] columns)
            {
                foreach (var column in columns)
                    list.Add(column);
                return this;
            }

            /// <summary>
            /// 修改列类型（列名，新类型定义）
            /// </summary>
            /// <param name="nameTypePairs"></param>
            /// <returns></returns>
            public SqlBuilderAlter ChangeColumnType(params (string, string)[] nameTypePairs)
            {
                addColumn = false;
                foreach (var pair in nameTypePairs)
                {
                    if (dic.ContainsKey(pair.Item1)) continue;
                    dic.Add(pair.Item1, pair.Item2);
                }
                return this;
            }

            /// <summary>
            /// 修改列（列名，新列名，新类型定义）
            /// </summary>
            /// <param name="column"></param>
            /// <param name="newColumnName"></param>
            /// <param name="newColumnType"></param>
            /// <returns></returns>
            public SqlBuilderAlter ChangeColumn(string column, string newColumnName, string newColumnType)
            {
                targetType = TargetType.ChangeColumn;

                target = column;
                newName = newColumnName;
                newType = newColumnType;
                return this;
            }

            /// <summary>
            /// 重命名表（新表名）
            /// </summary>
            /// <param name="newTableName"></param>
            /// <returns></returns>
            public SqlBuilderAlter RenameTable(string newTableName)
            {
                targetType = TargetType.RenameTable;

                target = newTableName;
                return this;
            }

            /// <summary>
            /// 设置唯一值列（列名）
            /// </summary>
            /// <param name="uniqueColumn"></param>
            /// <returns></returns>
            public SqlBuilderAlter SetUnique(string uniqueColumn)
            {
                targetType = TargetType.SetUnique;

                target = uniqueColumn;
                return this;
            }

            /// <summary>
            /// 移除列唯一值（列名）
            /// </summary>
            /// <param name="uniqueColumn"></param>
            /// <returns></returns>
            public SqlBuilderAlter DropUnique(string uniqueColumn)
            {
                targetType = TargetType.DropUnique;

                target = uniqueColumn;
                return this;
            }

        }

    }
}
