using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace test
{
    class cacheTableForMysql
    {
        private String findSql = @"SELECT * FROM ";
        private ArrayList sqlTable = null;
        private MySqlConnection connectHandle = null;
        private String table = "";
        private Boolean isCached = false;
        private String primaryKey = "";
        private Boolean prkeyIsSet = false;

        private String getColumns()
        {
            String columns = "";
            int count = 0;
            MySqlCommand mysqlCmd = new MySqlCommand(findSql, connectHandle);
            using (MySqlDataReader reader = mysqlCmd.ExecuteReader())
            {
                reader.Read();
                while (true)
                {
                    try
                    {
                        if (reader.GetName(count) == "id")
                        {
                            setPrkey(reader.GetName(count));
                            prkeyIsSet = true;
                        }
                        columns += reader.GetName(count++);
                        columns += "|";
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            return columns + "____status";
        }

        private Boolean scKernel(IDictionary<String, String> sc, IDictionary<String, String> temp, String[] par)
        {
            for (int i = 0; i < par.Length; i++)
            {
                try
                {
                    if (sc[par[i]] != temp[par[i]])
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return true;
        }

        private Boolean upKernel(ArrayList up, IDictionary<String, String> temp, String[] par)
        {
            try
            {
                foreach (int count in up)
                {
                    for (int i = 0; i < par.Length; i++)
                    {
                        ((IDictionary<String, String>)sqlTable[count])[par[i]] = temp[par[i]];
                    }
                    ((IDictionary<String, String>)sqlTable[count])["____status"] = "U";
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private Boolean createKernel(IDictionary<String, String> createItem)
        {
            connectHandle.Open();
            String columns = "";
            String value = "";
            foreach (String key in createItem.Keys)
            {
                if (key == "____status")
                {
                    continue;
                }
                columns += key + ",";
                value += "'" + createItem[key] + "'" + ",";
            }
            String sql = @"INSERT INTO " + table + " (" + columns.TrimEnd(',') + ") VALUES(" + value.TrimEnd(',') + ")";
            MySqlCommand mysqlCmd = new MySqlCommand(sql, connectHandle);
            try
            {
                mysqlCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                connectHandle.Close();
                return false;
            }
            connectHandle.Close();
            return true;
        }

        private Boolean deleteKernel(IDictionary<String, String> deleteItem)
        {
            connectHandle.Open();
            int length = deleteItem.Keys.Count;
            int count = 0;
            String[] columns = new String[length - 1];
            foreach (String key in deleteItem.Keys)
            {
                if (key == "____status")
                {
                    continue;
                }
                columns[count++] = key + " = " + "'" + deleteItem[key] + "'";
            }
            String deleteFrom = String.Join(" AND ", columns);
            String sql = @"DELETE FROM " + table + @" WHERE " + deleteFrom;
            MySqlCommand mysqlCmd = new MySqlCommand(sql, connectHandle);
            try
            {
                mysqlCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                connectHandle.Close();
                return false;
            }
            connectHandle.Close();
            return true;
        }

        private Boolean updateKernel(IDictionary<String, String> updateItem)
        {
            connectHandle.Open();
            int length = updateItem.Keys.Count;
            int count = 0;
            String[] columns = new String[length - 2];
            foreach (String key in updateItem.Keys)
            {
                if (key == primaryKey)
                {
                    continue;
                }
                if (key == "____status")
                {
                    continue;
                }
                columns[count++] = key + " = " + "'" + updateItem[key] + "'";
            }
            String updateFrom = String.Join(" , ", columns);
            String sql = "UPDATE " + table + " SET " + updateFrom + " WHERE " + primaryKey + " = " + updateItem[primaryKey];
            MySqlCommand mysqlCmd = new MySqlCommand(sql, connectHandle);
            try
            {
                mysqlCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                connectHandle.Close();
                return false;
            }
            connectHandle.Close();
            return true;
        }

        public cacheTableForMysql(MySqlConnection mysqlC, String tableName, Boolean cacheNow = true)
        {
            connectHandle = mysqlC;
            table = tableName;
            findSql += @"`" + tableName + @"`";
            if (cacheNow)
            {
                cache();
            }
        }

        public Boolean getPrkeyStatus()
        {
            prkeyIsSet = true;
            return prkeyIsSet;
        }

        public Boolean getCacheStatus()
        {
            return isCached;
        }

        public void setPrkey(String prkey)
        {
            primaryKey = prkey;
        }

        public void cache()
        {
            sqlTable = new ArrayList();
            connectHandle.Open();
            char[] delimiterChars = { '|' };
            String[] columns = getColumns().Split(delimiterChars);
            int columnsLength = columns.Length;
            MySqlCommand mysqlCmd = new MySqlCommand(findSql, connectHandle);
            using (MySqlDataReader reader = mysqlCmd.ExecuteReader())
            {
                try
                {
                    while (reader.Read())
                    {
                        IDictionary<String, String> row = new Dictionary<String, String>();
                        for (int i = 0; i < columnsLength - 1; i++)
                        {
                            row[reader.GetName(i).ToString()] = reader[i].ToString();
                        }
                        row["____status"] = "";
                        sqlTable.Add(row);
                    }
                }
                catch (Exception)
                {
                    isCached = false;
                    return;
                }
            }
            connectHandle.Close();
            isCached = true;
        }

        public IDictionary<String, String>[] read(params String[] screeningCondition)
        {
            if (!getCacheStatus())
            {
                return null;
            }
            if (screeningCondition.Length % 2 != 0)
            {
                return null;
            }
            ArrayList al = new ArrayList();
            IDictionary<String, String> sc = new Dictionary<String, String>();
            int length = screeningCondition.Length;
            String[] par = new String[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                sc[screeningCondition[i]] = screeningCondition[i + 1];
                par[i / 2] = screeningCondition[i];
            }
            foreach (IDictionary<String, String> temp in sqlTable)
            {
                if (!scKernel(sc, temp, par))
                {
                    continue;
                }
                if (temp["____status"] == "D")
                {
                    continue;
                }
                al.Add(temp);
            }
            int blLength = al.Count;
            int count = 0;
            IDictionary<String, String>[] result = new Dictionary<String, String>[blLength];
            foreach (IDictionary<String, String> temp in al)
            {
                result[count++] = temp;
            }
            return result;
        }

        public Boolean create(params String[] screeningCondition)
        {
            if (!getCacheStatus())
            {
                return false;
            }
            if (screeningCondition.Length % 2 != 0)
            {
                return false;
            }
            try
            {
                IDictionary<String, String> row = new Dictionary<String, String>();
                int length = screeningCondition.Length;
                String[] par = new String[length / 2];
                for (int i = 0; i < length; i += 2)
                {
                    row[screeningCondition[i]] = screeningCondition[i + 1];
                    par[i / 2] = screeningCondition[i];
                }
                row["____status"] = "C";
                sqlTable.Add(row);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public Boolean delete(params String[] screeningCondition)
        {
            if (!getCacheStatus())
            {
                return false;
            }
            if (screeningCondition.Length % 2 != 0)
            {
                return false;
            }
            try
            {
                IDictionary<String, String> sc = new Dictionary<String, String>();
                int length = screeningCondition.Length;
                String[] par = new String[length / 2];
                for (int i = 0; i < length; i += 2)
                {
                    sc[screeningCondition[i]] = screeningCondition[i + 1];
                    par[i / 2] = screeningCondition[i];
                }
                int count = 0;
                foreach (IDictionary<String, String> temp in sqlTable)
                {
                    if (!scKernel(sc, temp, par))
                    {
                        count++;
                        continue;
                    }
                    if (temp["____status"] == "D")
                    {
                        count++;
                        continue;
                    }
                    ((IDictionary<String, String>)sqlTable[count])["____status"] = "D";
                    count++;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public Boolean update(String findStr, String updateStr)
        {
            if (!getCacheStatus())
            {
                return false;
            }
            char[] delimiterChars = { ' ' };
            String[] findArr = findStr.Split(delimiterChars);
            String[] updateArr = updateStr.Split(delimiterChars);
            String[] findPar = new String[findArr.Length / 2];
            String[] updatePar = new String[updateArr.Length / 2];
            IDictionary<String, String> find = new Dictionary<String, String>();
            IDictionary<String, String> update = new Dictionary<String, String>();
            if (findArr.Length % 2 != 0)
            {
                return false;
            }
            if (updateArr.Length % 2 != 0)
            {
                return false;
            }
            for (int i = 0; i < findArr.Length; i += 2)
            {
                find[findArr[i]] = findArr[i + 1];
                findPar[i / 2] = findArr[i];
            }
            for (int i = 0; i < updateArr.Length; i += 2)
            {
                update[updateArr[i]] = updateArr[i + 1];
                updatePar[i / 2] = updateArr[i];
            }
            ArrayList al = new ArrayList();
            int count = 0;
            foreach (IDictionary<String, String> temp in sqlTable)
            {
                if (!scKernel(find, temp, findPar))
                {
                    count++;
                    continue;
                }
                if (temp["____status"] == "D")
                {
                    count++;
                    continue;
                }
                al.Add(count);
                count++;
            }
            if (!upKernel(al, update, updatePar))
            {
                return false;
            }
            return true;
        }

        public void showTableCache()
        {
            foreach (IDictionary<String, String> temp in sqlTable)
            {
                foreach (String key in temp.Keys)
                {
                    Console.Write("|" + temp[key] + "| ");
                }
                Console.WriteLine();
            }
        }

        public Boolean saveToDb()
        {
            foreach (IDictionary<String, String> temp in sqlTable)
            {
                Boolean status = true;
                if (temp["____status"] == "")
                {
                    continue;
                }
                if (temp["____status"] == "C")
                {
                    status = createKernel(temp);
                    continue;
                }
                if (temp["____status"] == "U")
                {
                    status = updateKernel(temp);
                    continue;
                }
                if (temp["____status"] == "D")
                {
                    status = deleteKernel(temp);
                    continue;
                }
                if (!status)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
 