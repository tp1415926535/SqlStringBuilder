# SqlStringBuilder
Generate sql statements by freely calling functions instead of manually splicing strings.     
通过自由调用函数生成sql语句，而不用手动拼接字符串。   

## Instructions
[Nuget_Package_Link](https://www.nuget.org/packages/SqlStringBuilder/1.0.0)   

Just reference the Release's Dll file.      
引用Release的Dll文件即可。    

## Examples
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

string insertSql = new SqlBuilder().Table("table").Insert().ColumnValues(("Column1", "1"), ("Column2", "2")).ReturnEffectCount("id").ToString();    
Console.WriteLine(insertSql);    
//insert into table (Column1 , Column2 ) values ('1','2')  returning id    

string updateSql = new SqlBuilder().Table("table").Update().ColumnValues(("Column1", "1"), ("Column2", "2")).Where(("id", "1")).ReturnEffectCount("id").ToString();    
Console.WriteLine(updateSql);    
//update table set Column1 ='1',Column2 ='2' where id ='1'  returning id    

string deleteSql = new SqlBuilder().Table("table").Delete().Where(("id", "1")).ReturnEffectCount("id").ToString();    
Console.WriteLine(deleteSql);    
//delete from table where id ='1'  returning id    

string checkTableExistSql = new SqlBuilder().Table("sqlite_master").Read().Columns("name").Where(("type", "table"), ("name", "tableName")).ToString();    
Console.WriteLine(checkTableExistSql);    
//select name from sqlite_master where type ='table' and name ='tableName'     

string removeTableSql = new SqlBuilder().Table("table").Drop().ToString();    
Console.WriteLine(removeTableSql);    
//drop table if exists table    

string backupSql = new SqlBuilder().Table("table").Read().Columns("Column1", "Column2").Copy("targetTable").ToString();        
Console.WriteLine(backupSql);    
//select into targetTable from table    

string createTableSql = new SqlBuilder().Table("table").Create().ColumnType(("Column1", "integer"), ("Column2", "integer")).ToString();    
Console.WriteLine(createTableSql);    
//create table table (Column1 integer,Column2 integer)    

string changeColumn = new SqlBuilder().Table("table").Alter().ChangeColumn("Column1", "C1", "integer").ToString();    
Console.WriteLine(changeColumn);    
//alter table  change column Column1 C1 integer    
```
