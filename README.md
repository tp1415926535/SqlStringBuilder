# SqlStringBuilder
Generate sql statements by freely calling functions instead of manually splicing strings.     
通过自由调用函数生成sql语句，而不用手动拼接字符串。   

[![release](https://img.shields.io/static/v1?label=release&message=1.0.4&color=green&logo=github)](https://github.com/tp1415926535/SqlStringBuilder/releases) 
[![nuget](https://img.shields.io/static/v1?label=nuget&message=1.0.4&color=lightblue&logo=nuget)](https://www.nuget.org/packages/SqlStringBuilder) 
[![license](https://img.shields.io/static/v1?label=license&message=MIT&color=silver)](https://github.com/tp1415926535/SqlStringBuilder/blob/master/LICENSE) 
![C#](https://img.shields.io/github/languages/top/tp1415926535/SqlStringBuilder) 

## Instructions
Reference the Release dll file or download the nuget package.    
引用Release的dll文件或下载nuget包。     

```C#
using SqlStringBuilder;

string sql = new SqlBuilder()
                .Table("table")
                //Add command like Read() or Insert() etc... 
                .ToString();   
``` 
The order in which the methods are called is unaffected, except for the base functions(Read,Insert,Update,etc).     
除了基础函数(Read,Insert,Update等)，其他方法的调用顺序不受影响。

## Examples

### Select

```C#
string readSql = new SqlBuilder().Table("table").Read().Distinct().Columns("Column1", "Column2").Where(("Column1", "1"), ("Column2", "2")).Order(("id", false)).ToString();     
Console.WriteLine(readSql);    
//select  distinct Column1,Column2 from table where Column1 ='1' and Column2 ='2'  order by id desc   

string readOperatorSql = new SqlBuilder().Table("table").Read().Where(WhereOperator.Less, true, false, ("Column1", "10"), ("Column2", "5")).ToString();
Console.WriteLine(readOperatorSql);  
//select  *  from table where Column1 <10 or  Column2 <5

string readBetweenSql = new SqlBuilder().Table("table").Read().WhereBetween("Column1","50","100").ToString();
Console.WriteLine(readBetweenSql);  
//select  *  from table where Column1 between '50' and '100'

string readLastRowSql = new SqlBuilder().Table("table").Read().Order(("id", false)).Limit(1).ToString();    
Console.WriteLine(readLastRowSql);    
//select  *  from table order by id desc  limit 1    

string getCountSql = new SqlBuilder().Table("table").Read().Count().ToString();     
Console.WriteLine(getCountSql);     
//select  count(  * ) from table    
```

### Insert / Update / Delete
```C#
string insertSql = new SqlBuilder().Table("table").Insert().ColumnValues(("Column1", "1"), ("Column2", "2")).ReturnEffectCount("id").ToString();    
Console.WriteLine(insertSql);    
//insert into table (Column1 , Column2 ) values ('1','2')  returning id    

string updateSql = new SqlBuilder().Table("table").Update().ColumnValues(("Column1", "1"), ("Column2", "2")).Where(("id", "1")).ReturnEffectCount("id").ToString();    
Console.WriteLine(updateSql);    
//update table set Column1 ='1',Column2 ='2' where id ='1'  returning id    

string deleteSql = new SqlBuilder().Table("table").Delete().Where(("id", "1")).ReturnEffectCount("id").ToString();    
Console.WriteLine(deleteSql);    
//delete from table where id ='1'  returning id    
```

and you can also set dictionary or datatable as paramater:
```C#
Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
keyValuePairs.Add("Column1", "1");
keyValuePairs.Add("Column2", "2");
keyValuePairs.Add("Column3", "3");
string insertDic = new SqlBuilder().Table("table").Insert().ColumnValues(keyValuePairs).ToString();
Console.WriteLine(insertDic);
//insert into table (Column1 , Column2 , Column3 ) values ('1','2','3') 

DataTable dataTable = new DataTable();
dataTable.Columns.Add("Column1"); dataTable.Columns.Add("Column2");
var row1 = dataTable.NewRow(); row1["Column1"] = "1-1"; row1["Column2"] = "1-2"; dataTable.Rows.Add(row1);
var row2 = dataTable.NewRow(); row2["Column1"] = "2-1"; row2["Column2"] = "2-2"; dataTable.Rows.Add(row2);
string insertData = new SqlBuilder().Table("table").Insert().ColumnValues(dataTable).ToString();
Console.WriteLine(insertData);
//insert into table(Column1,Column2) values ('1-1','1-2'),('2-1','2-2')
```

Or even use custom class instance (class properties should be set public and "{get;set;}" ) 
```C#
class Foo
{
    public string col1 { get; set; }
    public string col2 { get; set; }
}

Foo foo = new Foo { col1 = "1", col2 = "2" };
string insertClass = new SqlBuilder().Table("table").Insert().ColumnValuesFromClass(foo).ToString();
Console.WriteLine(insertClass);
//insert into table (col1 , col2 ) values ('1','2') 

List<Foo> classes = new List<Foo>();
classes.Add(new Foo { col1 = "a1", col2 = "a2" });
classes.Add(new Foo { col1 = "b1", col2 = "b2" });
classes.Add(new Foo { col1 = "c1", col2 = "c2" });
string insertClasses = new SqlBuilder().Table("table").Insert().ColumnValuesFromClasses(classes).ToString();
Console.WriteLine(insertClasses);
//insert into table(col1,col2) values ('a1','a2'),('b1','b2'),('c1','c2')
```

### Edit table structure
```C#
string createTableSql = new SqlBuilder().Table("table").Create().ColumnType(("Column1", "integer"), ("Column2", "integer")).ToString();    
Console.WriteLine(createTableSql);    
//create table table (Column1 integer,Column2 integer)    

string changeColumn = new SqlBuilder().Table("table").Alter().ChangeColumn("Column1", "C1", "integer").ToString();    
Console.WriteLine(changeColumn);    
//alter table  change column Column1 C1 integer    
```

### View
```C#
string createOrUpdateViewSql = new SqlBuilder().Table("table").Read().Columns("Column1","Column2").AsView("viewName").ToString();
Console.WriteLine(createOrUpdateViewSql);  
//create or replace view 'viewName' as select Column1,Column2 from table

string createViewFromMultySql = new SqlBuilder().Table("table1,table2").Read().Columns("table1.* " , "table2.Name 'Name'" , "table2.Value 'Value'").AsView("viewName").ToString();
Console.WriteLine(createViewFromMultySql);  
//create or replace view 'viewName' as select table1.* ,table2.Name 'Name',table2.Value 'Value' from table1,table2

string dropViewSql = new SqlBuilder().Table("viewName").Drop(true, true);
Console.WriteLine(dropViewSql);  
//drop view if exists viewName
```
    
### Other operations
```C#
string checkTableExistSql = new SqlBuilder().Table("sqlite_master").Read().Columns("name").Where(("type", "table"), ("name", "tableName")).ToString();    
Console.WriteLine(checkTableExistSql);    
//select name from sqlite_master where type ='table' and name ='tableName'     

string checkTableExistSql2 = new SqlBuilder().Table("tableName").CheckExist();
Console.WriteLine(checkTableExistSql2);
//select name from sqlite_master where type='table' and name='tableName'

string getAllTableSql = new SqlBuilder().Table("sqlite_master").Read().Columns("name").Where(("type", "table")).ToString();
Console.WriteLine(getAllTableSql);
//select name from sqlite_master where type ='table' 

string getAllTableSql2 = new SqlBuilder().GetAllNames();
Console.WriteLine(getAllTableSql2);
//select name from sqlite_master where type='table'

string getAllViewSql = new SqlBuilder().GetAllNames(true);
Console.WriteLine(getAllViewSql);
//select name from sqlite_master where type='view'

string backupSql = new SqlBuilder().Table("table").Read().Columns("Column1", "Column2").Copy("targetTable").ToString();        
Console.WriteLine(backupSql);    
//select Column1,Column2 into targetTable from table

string removeTableSql = new SqlBuilder().Table("tableName").Drop().ToString();    
Console.WriteLine(removeTableSql);    
//drop table if exists tableName    
```



## Version 
* v1.0.4: 2022/10/31   Compatible View, adjust 'where' method, add demo project. 兼容视图，调整where方法，增加示例项目。   
* v1.0.3: 2022/08/25   Fix the way the select into statement is generated and the results. 修复select into语句的生成方式和结果。
* v1.0.2: 2022/08/23   Additional comment generation.The comment is now displayed when the mouse hovers over the Read(), Insert(), Update(), ... , ToString(), etc. methods will now display comments. 补充注释生成。现在鼠标停留在 Read()，Insert()，Update()，...，ToString()等方法时会显示注释。 
* v1.0.1: 2022/08/23   Fixed incorrect dataTable copying. 修复了错误的数据表复制。
* v1.0.0: 2022/08/22   Basic version. 基础版本
