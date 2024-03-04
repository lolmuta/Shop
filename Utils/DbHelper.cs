using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using WinformTable.Ext;
using System.Reflection;

namespace Shop.Utils
{
    public class DbHelper
    {
        private readonly IConfiguration _configuration;
        
        public DbHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public T ConnDb<T>(Func<SqlConnection, T> func)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using (var sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                return func(sqlconn);
            }
        }
        public void ConnDb(Action<SqlConnection> act)
        {
            ConnDb(act.ToFunc());
        }
        public DataTable Query(string sqlQuery, Action<SqlParameterCollection> paramAct)
        {
            DataTable dataTable = new DataTable();
            var connectionString = _configuration.GetConnectionString("Database");
            using (var sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, sqlconn))
                {
                    paramAct?.Invoke(command.Parameters);
                    // 创建一个 SqlDataAdapter 并使用它填充 DataTable
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }
        public List<T> Query<T>(string sqlQuery, Action<SqlParameterCollection> paramAct) where T : new()
        {
            DataTable dataTable = Query(sqlQuery, paramAct);
           
            return ConvertToList<T>(dataTable);
        }
        public T ExecuteScalar<T>(string sqlQuery, Action<SqlParameterCollection> paramAct)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using (var sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, sqlconn))
                {
                    paramAct?.Invoke(command.Parameters);
                    // 执行查询并返回结果
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return (T)Convert.ChangeType(result, typeof(T));
                    }
                    else
                    {
                        // 如果结果为 null，则返回默认值
                        return default(T);
                    }
                }
            }
        }
        public int ExecuteNonQuery(string sqlExecute, Action<SqlParameterCollection> paramAct)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using (var sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                using (SqlCommand command = new SqlCommand(sqlExecute, sqlconn))
                {
                    paramAct?.Invoke(command.Parameters);
                    // 创建一个 SqlDataAdapter 并使用它填充 DataTable
                    return command.ExecuteNonQuery();
                }
            }
        }
        public string GetPageSql(string sqlQuery, int PageSize, int PageNumber)
        {
            int start = (PageNumber - 1) * PageSize + 1;
            int end = start + PageSize;
            var sqlPageQuery = $@"
                select a.* from ({sqlQuery}) a 
                where a.RowNum>={start} and a.RowNum<{end}";
            return sqlPageQuery;
        }
        public int GetTotalPage(string sqlQuery, Action<SqlParameterCollection> paramAct, int PageSize)
        {
            var sqlDataCount = $@"select count(0) from ({sqlQuery}) a ";
            var dataCount = ExecuteScalar<int>(sqlDataCount, paramAct);
            var totalPage = dataCount / PageSize;
            if (dataCount % PageSize > 0)
            {
                totalPage++;
            }
            return totalPage;
        }
        public int GetDataCount(string sqlQuery, Action<SqlParameterCollection> paramAct)
        {
            var sqlDataCount = $@"select count(0) from ({sqlQuery}) a ";
            var dataCount = ExecuteScalar<int>(sqlDataCount, paramAct);
            return dataCount;
        }
        public List<T> ConvertToList<T>(DataTable dataTable) where T : new()
        {
            List<T> list = new List<T>();

            foreach (DataRow row in dataTable.Rows)
            {
                T item = ConvertToObject<T>(row);
                list.Add(item);
            }

            return list;
        }
        internal T ConvertToObject<T>(DataRow row) where T : new()
        {
            T obj = new T();

            foreach (var memberInfo in typeof(T).GetMembers())
            {
                if (memberInfo is PropertyInfo property)
                {
                    SetPropertyValue(property, obj, row);
                }
                else if (memberInfo is FieldInfo field)
                {
                    SetFieldValue(field, obj, row);
                }
            }

            return obj;
        }

        private void SetPropertyValue(PropertyInfo property, object obj, DataRow row)
        {
            if (row.Table.Columns.Contains(property.Name))
            {
                object value = row[property.Name];

                if (value != DBNull.Value)
                {
                    if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                    {
                        // 处理可空类型属性
                        property.SetValue(obj, Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType)));
                    }
                    else
                    {
                        // 处理非可空类型属性
                        property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                    }
                }
                else
                {
                    // 属性值为 DBNull.Value，设置为 null 或默认值
                    property.SetValue(obj, property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null);
                }
            }
        }

        private void SetFieldValue(FieldInfo field, object obj, DataRow row)
        {
            if (row.Table.Columns.Contains(field.Name))
            {
                object value = row[field.Name];

                if (value != DBNull.Value)
                {
                    if (Nullable.GetUnderlyingType(field.FieldType) != null)
                    {
                        // 处理可空类型字段
                        field.SetValue(obj, Convert.ChangeType(value, Nullable.GetUnderlyingType(field.FieldType)));
                    }
                    else
                    {
                        // 处理非可空类型字段
                        field.SetValue(obj, Convert.ChangeType(value, field.FieldType));
                    }
                }
                else
                {
                    // 字段值为 DBNull.Value，设置为 null 或默认值
                    field.SetValue(obj, field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : null);
                }
            }
        }


    }
}
