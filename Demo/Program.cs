using SqlStringBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            string readSql = new SqlBuilder()
                .Table("table")
                .Read()
                .Distinct()
                .Columns("Column1", "Column2")
                .Where(("Column1", "1"), ("Column2", "2"))
                .Order(("id", false))
                .ToString();
            Console.WriteLine("\r\n read:\r\n" + readSql);

            SelectExamples();
            InsertUpdateDeleteExamples();
            EditExamples();
            ViewExamples();
            OthersExamples();

            Console.Read();
        }

        private static void SelectExamples()
        {
            Console.WriteLine("\r\n————————————\r\nSelect\r\n————————————");

            string readAllSql = new SqlBuilder().Table("table").Read().ToString();
            Console.WriteLine("\r\n readAll:\r\n" + readAllSql);

            string readOperatorSql = new SqlBuilder().Table("table").Read().Where(WhereOperator.Less, true, false, ("Column1", "10"), ("Column2", "5")).ToString();
            Console.WriteLine("\r\n readOperator:\r\n" + readOperatorSql);

            string readBetweenSql = new SqlBuilder().Table("table").Read().WhereBetween("Column1","50","100").ToString();
            Console.WriteLine("\r\n readBetween:\r\n" + readBetweenSql);

            string readLastRowSql = new SqlBuilder().Table("table").Read().Order(("id", false)).Limit(1).ToString();
            Console.WriteLine("\r\n readLastRow:\r\n" + readLastRowSql);

            string getCountSql = new SqlBuilder().Table("table").Read().Count().ToString();
            Console.WriteLine("\r\n getCount:\r\n" + getCountSql);
        }

        private static void InsertUpdateDeleteExamples()
        {
            Console.WriteLine("\r\n————————————\r\nInsert/Update/Delete\r\n————————————");

            string insertSql = new SqlBuilder().Table("table").Insert().ColumnValues(("Column1", "1"), ("Column2", "2")).ReturnEffectCount("id").ToString();
            Console.WriteLine("\r\n insert:\r\n" + insertSql);

            string updateSql = new SqlBuilder().Table("table").Update().ColumnValues(("Column1", "1"), ("Column2", "2")).Where(("id", "1")).ReturnEffectCount("id").ToString();
            Console.WriteLine("\r\n update:\r\n" + updateSql);

            string deleteSql = new SqlBuilder().Table("table").Delete().Where(("id", "1")).ReturnEffectCount("id").ToString();
            Console.WriteLine("\r\n delete:\r\n" + deleteSql);

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add("Column1", "1");
            keyValuePairs.Add("Column2", "2");
            keyValuePairs.Add("Column3", "3");
            string insertDic = new SqlBuilder().Table("table").Insert().ColumnValues(keyValuePairs).ToString();
            Console.WriteLine("\r\n insertDic:\r\n" + insertDic);

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Column1"); dataTable.Columns.Add("Column2");
            var row1 = dataTable.NewRow(); row1["Column1"] = "1-1"; row1["Column2"] = "1-2"; dataTable.Rows.Add(row1);
            var row2 = dataTable.NewRow(); row2["Column1"] = "2-1"; row2["Column2"] = "2-2"; dataTable.Rows.Add(row2);
            string insertData = new SqlBuilder().Table("table").Insert().ColumnValues(dataTable).ToString();
            Console.WriteLine("\r\n insertTable:\r\n" + insertData);

            Foo foo = new Foo { col1 = "1", col2 = "2" };
            string insertClass = new SqlBuilder().Table("table").Insert().ColumnValuesFromClass(foo).ToString();
            Console.WriteLine("\r\n insertClass:\r\n" + insertClass);

            List<Foo> classes = new List<Foo>();
            classes.Add(new Foo { col1 = "a1", col2 = "a2" });
            classes.Add(new Foo { col1 = "b1", col2 = "b2" });
            classes.Add(new Foo { col1 = "c1", col2 = "c2" });
            string insertClasses = new SqlBuilder().Table("table").Insert().ColumnValuesFromClasses(classes).ToString();
            Console.WriteLine("\r\n insertClasses:\r\n" + insertClasses);
        }

        private static void EditExamples()
        {
            Console.WriteLine("\r\n————————————\r\nEditSchema\r\n————————————");

            string createTableSql = new SqlBuilder().Table("table").Create().ColumnType(("Column1", "integer"), ("Column2", "integer")).ToString();
            Console.WriteLine("\r\n createTable:\r\n" + createTableSql);

            string changeColumn = new SqlBuilder().Table("table").Alter().ChangeColumn("Column1", "C1", "integer").ToString();
            Console.WriteLine("\r\n changeColumn:\r\n" + changeColumn);
        }

        private static void ViewExamples()
        {
            Console.WriteLine("\r\n————————————\r\nView\r\n————————————");

            string createOrUpdateViewSql = new SqlBuilder().Table("table").Read().Columns("Column1","Column2").AsView("viewName").ToString();
            Console.WriteLine("\r\n createOrUpdateView:\r\n" + createOrUpdateViewSql);

            string createViewFromMultySql = new SqlBuilder().Table("table1,table2").Read().Columns("table1.* " , "table2.Name 'Name'" , "table2.Value 'Value'").AsView("viewName").ToString();
            Console.WriteLine("\r\n createViewFromMulty:\r\n" + createViewFromMultySql);

            string dropViewSql = new SqlBuilder().Table("viewName").Drop(true, true);
            Console.WriteLine("\r\n removeView:\r\n" + dropViewSql);
        }

        private static void OthersExamples()
        {
            Console.WriteLine("\r\n————————————\r\nOthers\r\n————————————");

            string checkTableExistSql = new SqlBuilder().Table("sqlite_master").Read().Columns("name").Where(("type", "table"), ("name", "tableName")).ToString();
            Console.WriteLine("\r\n checkTableExist1:\r\n" + checkTableExistSql);

            string checkTableExistSql2 = new SqlBuilder().Table("tableName").CheckExist();
            Console.WriteLine("\r\n checkTableExist2:\r\n" + checkTableExistSql2);

            string checkViewExistSql = new SqlBuilder().Table("viewName").CheckExist(true);
            Console.WriteLine("\r\n checkViewExist:\r\n" + checkViewExistSql);

            string getAllTableSql = new SqlBuilder().Table("sqlite_master").Read().Columns("name").Where(("type", "table")).ToString();
            Console.WriteLine("\r\n getAllTables:\r\n" + getAllTableSql);

            string getAllViewSql = new SqlBuilder().Table("sqlite_master").Read().Columns("name").Where(("type", "view")).ToString();
            Console.WriteLine("\r\n getAllViews:\r\n" + getAllViewSql);

            string backupSql = new SqlBuilder().Table("table").Read().Columns("Column1", "Column2").Copy("targetTable").ToString();
            Console.WriteLine("\r\n backup:\r\n" + backupSql);

            string removeTableSql = new SqlBuilder().Table("tableName").Drop().ToString();
            Console.WriteLine("\r\n removeTable:\r\n" + removeTableSql);
        }

        class Foo
        {
            public string col1 { get; set; }
            public string col2 { get; set; }
        }

    }
}
