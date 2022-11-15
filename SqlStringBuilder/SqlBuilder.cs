using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlStringBuilder
{
    /// <summary>
    /// 
    /// </summary>
    public enum WhereOperator
    {
        /// <summary>
        /// key = 'value'
        /// </summary>
        Equal,

        /// <summary>
        /// key ＞ 'value'
        /// </summary>
        Greater,

        /// <summary>
        /// key ＞= 'value'
        /// </summary>
        GreaterEqual,

        /// <summary>
        /// key ＜ 'value'
        /// </summary>
        Less,

        /// <summary>
        /// key ＜= 'value'
        /// </summary>
        LessEqual,

        /// <summary>
        /// key like 'value'
        /// </summary>
        Like,

    }

    /// <summary>
    /// SQL字符串构造器
    /// </summary>
    public partial class SqlBuilder
    {
        enum WhereType
        {
            Equal,
            Greater,
            GreaterEqual,
            Less,
            LessEqual,
            Like,
            Between,
            In,
            Exist,
        }
        enum WhereNestedMulty
        {
            None,
            Any,
            All,
        }

        string tableName = string.Empty;
        List<string> list = new List<string>();
        Dictionary<string, string> dic = new Dictionary<string, string>();

        WhereType whereType = WhereType.Equal;
        bool whereOrConnect = false;
        bool whereNot = false;
        Dictionary<string, string> whereFilter = new Dictionary<string, string>();
        string whereColumn = string.Empty;
        string whereNestedString = string.Empty;
        WhereNestedMulty whereNestedMulty = WhereNestedMulty.None;
        bool whereNoQuoMark = false;

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
        /// <param name="isView">是否为视图</param>
        /// <returns>drop table [ if exists ]tableName;</returns>
        /// <remarks>需要带上Table(tableName)</remarks>
        public string Drop(bool checkExist = true, bool isView = false)
        {
            string type = isView ? "view" : "table";
            string str = string.Empty;
            if (checkExist)
                str = "drop " + type + " if exists " + tableName;
            else
                str = "drop " + type + " " + tableName;
            return str;
        }



        /// <summary>
        /// 判断表是否存在
        /// </summary>
        /// <param name="isView">是否为视图</param>
        /// <returns>select name from sqlite_master where type='table' and name = 'tableName' </returns>
        /// <remarks>需要带上Table(tableName)</remarks>
        public string CheckExist(bool isView = false)
        {
            string type = isView ? "view" : "table";
            return "select name from sqlite_master where type='" + type + "' and name='" + tableName + "'";
        }

        /// <summary>
        /// 获取数据库所有表/视图名
        /// </summary>
        /// <param name="isView">是否为视图</param>
        /// <returns>select name from sqlite_master where type='table'</returns>
        public string GetAllNames(bool isView = false)
        {
            string type = isView ? "view" : "table";
            return "select name from sqlite_master where type='" + type + "'";
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


        private void AppendWhereSentence(StringBuilder stringBuilder, SqlBuilder sqlBuilder)
        {
            var whereType = sqlBuilder.whereType;
            var whereOrConnect = sqlBuilder.whereOrConnect;
            var whereNot = sqlBuilder.whereNot;
            var whereColumn = sqlBuilder.whereColumn;
            var whereFilter = sqlBuilder.whereFilter;

            stringBuilder.Append(" where ");
            if (whereType != WhereType.Between && whereType != WhereType.In)
            {
                string connect = whereOrConnect ? " or  " : " and ";
                if ((whereType == WhereType.Equal || whereType == WhereType.Like) && !whereNoQuoMark)
                    connect = whereOrConnect ? "' or  " : "' and ";
                string operat = string.IsNullOrEmpty(whereNestedString) && !whereNoQuoMark ? " = '" : " = ";
                switch (whereType)
                {
                    case WhereType.Equal:
                        break;
                    case WhereType.Less:
                        operat = " <";
                        break;
                    case WhereType.LessEqual:
                        operat = " <=";
                        break;
                    case WhereType.Greater:
                        operat = " >";
                        break;
                    case WhereType.GreaterEqual:
                        operat = " >=";
                        break;
                    case WhereType.Like:
                        operat = string.IsNullOrEmpty(whereNestedString) && !whereNoQuoMark ? " like'" : " like ";
                        break;
                    case WhereType.Exist:
                        operat = " exist ";
                        break;
                }
                if (string.IsNullOrEmpty(whereNestedString))
                {
                    foreach (var pair in whereFilter)
                    {
                        if (whereNot) stringBuilder.Append(" not ");
                        stringBuilder.Append(pair.Key);
                        stringBuilder.Append(operat);
                        stringBuilder.Append(pair.Value);
                        stringBuilder.Append(connect);
                    }
                    stringBuilder = stringBuilder.Remove(stringBuilder.Length - 4, 4);
                }
                else
                {
                    if (whereNot) stringBuilder.Append(" not ");
                    if (whereType != WhereType.Exist)
                        stringBuilder.Append(whereColumn);
                    stringBuilder.Append(operat);
                    if (whereNestedMulty == WhereNestedMulty.Any)
                        stringBuilder.Append(" any ");
                    else if (whereNestedMulty == WhereNestedMulty.All)
                        stringBuilder.Append(" all ");
                    stringBuilder.Append("(");
                    stringBuilder.Append(whereNestedString);
                    stringBuilder.Append(") ");
                }
            }
            else if (whereType.Equals(WhereType.In))
            {
                if (string.IsNullOrEmpty(whereNestedString))
                {
                    stringBuilder.Append(whereColumn);
                    if (whereNot) stringBuilder.Append(" not ");
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
                    if (whereNot) stringBuilder.Append(" not ");
                    stringBuilder.Append(whereColumn);
                    stringBuilder.Append(" in (");
                    stringBuilder.Append(whereNestedString);
                    stringBuilder.Append(") ");
                }
            }
            else
            {
                stringBuilder.Append(whereColumn);
                if (whereNot) stringBuilder.Append(" not ");
                stringBuilder.Append(" between '");
                stringBuilder.Append(whereFilter.ElementAt(0).Key);
                stringBuilder.Append("' and '");
                stringBuilder.Append(whereFilter.ElementAt(1).Key);
                stringBuilder.Append("' ");
            }

        }


    }
}
