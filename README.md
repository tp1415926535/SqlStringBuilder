# SqlStringBuilder
Generate sql statements by freely calling functions instead of manually splicing strings.     
通过自由调用函数生成sql语句，而不用手动拼接字符串。   

[![release](https://img.shields.io/static/v1?label=release&message=1.0.1&color=green&logo=github)](https://github.com/tp1415926535/SqlStringBuilder/releases) 
[![nuget](https://img.shields.io/static/v1?label=nuget&message=1.0.1&color=lightblue&logo=nuget)](https://www.nuget.org/packages/SqlStringBuilder) 
[![license](https://img.shields.io/static/v1?label=license&message=MIT&color=silver)](https://github.com/tp1415926535/SqlStringBuilder/blob/master/LICENSE) 
![C#](https://img.shields.io/github/languages/top/tp1415926535/SqlStringBuilder) 

## Instructions
Quote the Release Dll file or download the nuget package.      
引用Release的Dll文件或下载nuget包。     

```C#
using SqlStringBuilder;

string sql = new SqlBuilder()
                .Table("table")
                //Add command like Read() or Insert() etc... 
                .ToString();   
``` 
## Examples

### Select
```C#
string readSql = new SqlBuilder().Table("table").Read().Distinct().Columns("Column1", "Column2").Where(("Column1", "1"), ("Column2", "2")).Order(("id", false)).ToString();     
Console.WriteLine(readSql);    
//select  distinct Column1,Column2 from table where Column1 ='1' and Column2 ='2'  order by id desc     

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
//insert into table(Column1,Column2) values ('1,1','1,2'),('2,1','2,2')
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

### Other operations
```C#
string checkTableExistSql = new SqlBuilder().Table("sqlite_master").Read().Columns("name").Where(("type", "table"), ("name", "tableName")).ToString();    
Console.WriteLine(checkTableExistSql);    
//select name from sqlite_master where type ='table' and name ='tableName'     

string checkTableExistSql2 = new SqlBuilder().Table("tableName").CheckExist();
Console.WriteLine(checkTableExistSql2);
//select name from sqlite_master where type='table' and name='tableName'


string backupSql = new SqlBuilder().Table("table").Read().Columns("Column1", "Column2").Copy("targetTable").ToString();        
Console.WriteLine(backupSql);    
//select into targetTable from table    

string removeTableSql = new SqlBuilder().Table("table").Drop().ToString();    
Console.WriteLine(removeTableSql);    
//drop table if exists table    
```
