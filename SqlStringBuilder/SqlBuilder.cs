using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlStringBuilder
{
    /// <summary>
    /// SQL字符串构造器
    /// </summary>
    public class SqlBuilder
    {
        enum WhereType
        {
            EqualAnd,
            EqualOr,
            LikeAnd,
            LikeOr,
            InMulty,
            Between,
            NotBetween,
        }
        //SqlAction action = SqlAction.None;
        string tableName = string.Empty;
        List<string> list = new List<string>();
        Dictionary<string, string> dic = new Dictionary<string, string>();
        WhereType whereType = WhereType.EqualAnd;
        Dictionary<string, string> whereFilter = new Dictionary<string, string>();
        string whereColumn = string.Empty;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SqlBuilder()
        {
        }

        /// <summary>
        /// 带表声明的构造函数
        /// </summary>
        /// <param name="dbTableName"></param>
        public SqlBuilder(string dbTableName)
        {
            tableName = dbTableName;
        }

        /// <summary>
        /// 使用读取模板
        /// </summary>
        /// <returns>select * from tableName...</returns>
        public SqlBuilderRead Read()
        {
            return new SqlBuilderRead(this);
        }

        /// <summary>
        /// 使用添加行模板
        /// </summary>
        /// <returns>insert into tableName (col1, ... ) values ('value1', ... )</returns>
        public SqlBuilderInsert Insert()
        {
            return new SqlBuilderInsert(this);
        }

        /// <summary>
        /// 使用修改行模板
        /// </summary>
        /// <returns>update tableName set col1 = 'value1', ... where xx = 'value'</returns>
        public SqlBuilderUpdate Update()
        {
            return new SqlBuilderUpdate(this);
        }

        /// <summary>
        /// 使用删除行模板
        /// </summary>
        /// <returns>delete from tabelName [ where xx = 'value' ]</returns>
        public SqlBuilderDelete Delete()
        {
            return new SqlBuilderDelete(this);
        }

        /// <summary>
        /// 使用创建表模板
        /// </summary>
        /// <returns>create table tableName (col1 type , ... )</returns>
        public SqlBuilderCreate Create()
        {
            return new SqlBuilderCreate(this);
        }

        /// <summary>
        /// 使用修改表结构模板
        /// </summary>
        /// <returns>alter table tableName...</returns>
        public SqlBuilderAlter Alter()
        {
            return new SqlBuilderAlter(this);
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="checkExist"></param>
        /// <returns>drop table [ if exists ]tableName;</returns>
        /// <remarks>需要带上Table(tableName)</remarks>
        public string Drop(bool checkExist = true)
        {
            string str = string.Empty;
            if (checkExist)
                str = "drop table if exists " + tableName;
            else
                str = "drop table " + tableName;
            return str;
        }


        /// <summary>
        /// 判断表是否存在
        /// </summary>
        /// <returns>select name from sqlite_master where type='table' and name = 'tableName' </returns>
        /// <remarks>需要带上Table(tableName)</remarks>
        public string CheckExist()
        {
            return "select name from sqlite_master where type='table' and name='" + tableName + "'";
        }


        /// <summary>
        /// 声明作用的表
        /// </summary>
        /// <param name="dbtableName"></param>
        /// <returns></returns>
        public SqlBuilder Table(string dbtableName)
        {
            tableName = dbtableName;
            return this;
        }

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
            }

            /// <summary>
            /// 转为字符串
            /// </summary>
            /// <returns>select * from tableName...</returns>
            public new string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
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
                if (whereFilter.Count > 0)
                {
                    stringBuilder.Append(" where ");
                    if (whereType != WhereType.NotBetween && whereType != WhereType.Between && whereType != WhereType.InMulty)
                    {
                        string combine1 = " ='"; string combine2 = "' and ";
                        switch (whereType)
                        {
                            case WhereType.EqualAnd:
                                break;
                            case WhereType.LikeAnd:
                                combine1 = " like'"; combine2 = "' and ";
                                break;
                            case WhereType.EqualOr:
                                combine1 = " ='"; combine2 = "' or  ";
                                break;
                            case WhereType.LikeOr:
                                combine1 = " like'"; combine2 = "' or  ";
                                break;
                        }
                        foreach (var pair in whereFilter)
                        {
                            stringBuilder.Append(pair.Key);
                            stringBuilder.Append(combine1);
                            stringBuilder.Append(pair.Value);
                            stringBuilder.Append(combine2);
                        }
                        stringBuilder = stringBuilder.Remove(stringBuilder.Length - 4, 4);
                    }
                    else if (whereType.Equals(WhereType.InMulty))
                    {
                        stringBuilder.Append(whereColumn);
                        stringBuilder.Append(" in ('");
                        foreach (string value in whereFilter.Keys)
                        {
                            stringBuilder.Append(value);
                            stringBuilder.Append("','");
                        }
                        stringBuilder = stringBuilder.Remove(stringBuilder.Length - 2, 2);
                        stringBuilder.Append(") ");
                    }
                    else
                    {
                        stringBuilder.Append(whereColumn);
                        if (whereType.Equals(WhereType.NotBetween))
                            stringBuilder.Append(" not ");
                        stringBuilder.Append(" between '");
                        stringBuilder.Append(whereFilter.ElementAt(0).Key);
                        stringBuilder.Append("' and '");
                        stringBuilder.Append(whereFilter.ElementAt(1).Key);
                        stringBuilder.Append("' ");
                    }
                }
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


            private void SetWhereFilter(params (string, string)[] keyValuePairs)
            {
                foreach (var pair in keyValuePairs)
                {
                    if (whereFilter.ContainsKey(pair.Item1)) continue;
                    whereFilter.Add(pair.Item1, pair.Item2);
                }
            }

            /// <summary>
            /// 筛选条件_相等_与（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderRead Where(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.EqualAnd;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_模糊_与（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderRead WhereLike(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.LikeAnd;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_相等_或（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderRead WhereOr(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.EqualOr;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_模糊_或（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderRead WhereLikeOr(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.LikeOr;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_多个值（筛选列，值）
            /// </summary>
            /// <param name="column"></param>
            /// <param name="values"></param>
            /// <returns></returns>
            public SqlBuilderRead WhereIn(string column, params string[] values)
            {
                whereType = WhereType.InMulty;
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
                whereType = not ? WhereType.NotBetween : WhereType.Between;
                whereColumn = column;
                if (!whereFilter.ContainsKey(value1))
                    whereFilter.Add(value1, string.Empty);
                if (!whereFilter.ContainsKey(value2))
                    whereFilter.Add(value2, string.Empty);
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

        }

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
                    stringBuilder.Append("(");
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
                if (whereFilter.Count > 0)
                {
                    stringBuilder.Append(" where ");
                    if (whereType != WhereType.NotBetween && whereType != WhereType.Between && whereType != WhereType.InMulty)
                    {
                        string combine1 = " ='"; string combine2 = "' and ";
                        switch (whereType)
                        {
                            case WhereType.EqualAnd:
                                break;
                            case WhereType.LikeAnd:
                                combine1 = " like'"; combine2 = "' and ";
                                break;
                            case WhereType.EqualOr:
                                combine1 = " ='"; combine2 = "' or  ";
                                break;
                            case WhereType.LikeOr:
                                combine1 = " like'"; combine2 = "' or  ";
                                break;
                        }
                        foreach (var pair in whereFilter)
                        {
                            stringBuilder.Append(pair.Key);
                            stringBuilder.Append(combine1);
                            stringBuilder.Append(pair.Value);
                            stringBuilder.Append(combine2);
                        }
                        stringBuilder = stringBuilder.Remove(stringBuilder.Length - 4, 4);
                    }
                    else if (whereType.Equals(WhereType.InMulty))
                    {
                        stringBuilder.Append(whereColumn);
                        stringBuilder.Append(" in ('");
                        foreach (string value in whereFilter.Keys)
                        {
                            stringBuilder.Append(value);
                            stringBuilder.Append("','");
                        }
                        stringBuilder = stringBuilder.Remove(stringBuilder.Length - 2, 2);
                        stringBuilder.Append(") ");
                    }
                    else
                    {
                        stringBuilder.Append(whereColumn);
                        if (whereType.Equals(WhereType.NotBetween))
                            stringBuilder.Append(" not ");
                        stringBuilder.Append(" between '");
                        stringBuilder.Append(whereFilter.ElementAt(0).Key);
                        stringBuilder.Append("' and '");
                        stringBuilder.Append(whereFilter.ElementAt(1).Key);
                        stringBuilder.Append("' ");
                    }
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

            private void SetWhereFilter(params (string, string)[] keyValuePairs)
            {
                foreach (var pair in keyValuePairs)
                {
                    if (whereFilter.ContainsKey(pair.Item1)) continue;
                    whereFilter.Add(pair.Item1, pair.Item2);
                }
            }

            /// <summary>
            /// 筛选条件_相等_与（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderUpdate Where(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.EqualAnd;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_模糊_与（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderUpdate WhereLike(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.LikeAnd;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_相等_或（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderUpdate WhereOr(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.EqualOr;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_模糊_或（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderUpdate WhereLikeOr(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.LikeOr;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_多个值（筛选列，值）
            /// </summary>
            /// <param name="column"></param>
            /// <param name="values"></param>
            /// <returns></returns>
            public SqlBuilderUpdate WhereIn(string column, params string[] values)
            {
                whereType = WhereType.InMulty;
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
                whereType = not ? WhereType.NotBetween : WhereType.Between;
                whereColumn = column;
                if (!whereFilter.ContainsKey(value1))
                    whereFilter.Add(value1, string.Empty);
                if (!whereFilter.ContainsKey(value2))
                    whereFilter.Add(value2, string.Empty);
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
                if (whereFilter.Count > 0)
                {
                    stringBuilder.Append(" where ");
                    if (whereType != WhereType.NotBetween && whereType != WhereType.Between && whereType != WhereType.InMulty)
                    {
                        string combine1 = " ='"; string combine2 = "' and ";
                        switch (whereType)
                        {
                            case WhereType.EqualAnd:
                                break;
                            case WhereType.LikeAnd:
                                combine1 = " like'"; combine2 = "' and ";
                                break;
                            case WhereType.EqualOr:
                                combine1 = " ='"; combine2 = "' or  ";
                                break;
                            case WhereType.LikeOr:
                                combine1 = " like'"; combine2 = "' or  ";
                                break;
                        }
                        foreach (var pair in whereFilter)
                        {
                            stringBuilder.Append(pair.Key);
                            stringBuilder.Append(combine1);
                            stringBuilder.Append(pair.Value);
                            stringBuilder.Append(combine2);
                        }
                        stringBuilder = stringBuilder.Remove(stringBuilder.Length - 4, 4);
                    }
                    else if (whereType.Equals(WhereType.InMulty))
                    {
                        stringBuilder.Append(whereColumn);
                        stringBuilder.Append(" in ('");
                        foreach (string value in whereFilter.Keys)
                        {
                            stringBuilder.Append(value);
                            stringBuilder.Append("','");
                        }
                        stringBuilder = stringBuilder.Remove(stringBuilder.Length - 2, 2);
                        stringBuilder.Append(") ");
                    }
                    else
                    {
                        stringBuilder.Append(whereColumn);
                        if (whereType.Equals(WhereType.NotBetween))
                            stringBuilder.Append(" not ");
                        stringBuilder.Append(" between '");
                        stringBuilder.Append(whereFilter.ElementAt(0).Key);
                        stringBuilder.Append("' and '");
                        stringBuilder.Append(whereFilter.ElementAt(1).Key);
                        stringBuilder.Append("' ");
                    }
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
            public new SqlBuilderDelete Table(string dbtableName)
            {
                tableName = dbtableName;
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
            /// 筛选条件_相等_与（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderDelete Where(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.EqualAnd;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_模糊_与（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderDelete WhereLike(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.LikeAnd;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_相等_或（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderDelete WhereOr(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.EqualOr;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_模糊_或（筛选列，筛选值）
            /// </summary>
            /// <param name="keyValuePairs"></param>
            /// <returns></returns>
            public SqlBuilderDelete WhereLikeOr(params (string, string)[] keyValuePairs)
            {
                whereType = WhereType.LikeOr;
                SetWhereFilter(keyValuePairs);
                return this;
            }

            /// <summary>
            /// 筛选条件_多个值（筛选列，值）
            /// </summary>
            /// <param name="column"></param>
            /// <param name="values"></param>
            /// <returns></returns>
            public SqlBuilderDelete WhereIn(string column, params string[] values)
            {
                whereType = WhereType.InMulty;
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
                whereType = not ? WhereType.NotBetween : WhereType.Between;
                whereColumn = column;
                if (!whereFilter.ContainsKey(value1))
                    whereFilter.Add(value1, string.Empty);
                if (!whereFilter.ContainsKey(value2))
                    whereFilter.Add(value2, string.Empty);
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
