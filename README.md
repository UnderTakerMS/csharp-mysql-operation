# csharp-mysql-operation      
一个支持事务带缓存的C# mysql操作库      
操作库会缓存指定数据表     

## 函数列表     
### cacheTableForMysql(MySqlConnection mysqlC, String tableName, Boolean cacheNow = true)    
构造函数需要传入一个MySqlConnection,数据库表名。cacheNow参数设置时候在构造时缓存     
### Boolean getPrkeyStatus()      
返回缓存表主键设置状态(true or false)       
### Boolean getCacheStatus()       
返回是否缓存(true or false)       
### void setPrkey(String prkey)      
设置缓存表主键，需要传入一个主键名称       
### void cache()      
缓存设置的数据表      
### IDictionary<String, String>[] read(params String[] screeningCondition)       
从缓存中读取一行或多行，需要传入双数个参数，字段名键值。(例如：read("id","1")--读取所有id=1的行)       
### Boolean create(params String[] screeningCondition)      
向缓存表中写入一行数据，需要传入双数个参数，使用方法与上面类似。返回是否成功true or false       
### Boolean delete(params String[] screeningCondition)      
从缓存表中删除一行或几行数据，需要传入双数个参数，传入参数为删除条件。返回是否成功true or false          
### Boolean update(String findStr, String updateStr)        
更新缓存表中的一行或几行数据，需要传入两个字符串。(例如：update("id 1 name 'xiaoming'","age 15"))，一个字符串需要传入双数个成员，前面字符串为搜索条件，后一字符串为更新项目。返回是否成功true or false           
### String[] find(IDictionary<String, String>[] resSet, String findStr, String columnsStr)      
从read函数返回的结果中按照也顶条件查找特定字段，第一个参数为read函数的返回结果，第二个参数为查找条件，第三个为返回的字段名。返回对应结果的字符串数组        
### void showTableCache()     
显示缓存表的全部内容       
### Boolean saveToDb()     
将缓存表保存到数据库，返回保存是否成功true or false       