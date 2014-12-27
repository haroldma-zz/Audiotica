#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SQLitePCL;

#endregion

namespace Audiotica.Data.Collection.SqlHelper
{
    public static class EasySql
    {
        public static readonly Dictionary<Type, string> NetToSqlKepMap = new Dictionary<Type, string>
        {
            {typeof (long), "INTEGER"},
            {typeof (bool), "INTEGER"},
            {typeof (TimeSpan), "BIGINT"},
            {typeof (int), "INTEGER"},
            {typeof (string), "TEXT"},
            {typeof (float), "REAL"},
            {typeof (DateTime), "DATETIME DEFAULT (datetime('now','localtime'))"},
            {typeof (double), "REAL"}
        };

        public static string CreateTable(Type type)
        {
            var props = type.GetRuntimeProperties()
                .Where(p => p.CustomAttributes.Count(n =>
                    n.AttributeType == typeof(SqlIgnore)) == 0 && (NetToSqlKepMap.ContainsKey(p.PropertyType) || p.PropertyType.GetTypeInfo().IsEnum));

            const string sql = "CREATE TABLE IF NOT EXISTS {0} ({1});";
            var name = type.Name;
            var collumns = "";
            var foreignKeys = "";

            foreach (var propertyInfo in props)
            {
                var first = collumns == "";
                var prefix = (first ? "" : ", ");

                //add naame
                var currentCollumn = prefix + propertyInfo.Name;

                //type
                if (propertyInfo.PropertyType.GetTypeInfo().IsEnum)
                    currentCollumn += " INTEGER";
                else
                    currentCollumn += " " + NetToSqlKepMap[propertyInfo.PropertyType];

                //any other prop
                var sqlProp = propertyInfo.GetCustomAttribute<SqlProperty>();

                if (sqlProp != null)
                {
                    if (sqlProp.IsNull)
                    {
                        currentCollumn = currentCollumn.Replace(" DEFAULT (datetime('now','localtime'))", "");
                    }

                    if (sqlProp.IsPrimaryKey)
                    {
                        currentCollumn += " PRIMARY KEY AUTOINCREMENT NOT NULL";
                    }

                    if (sqlProp.ReferenceTo != null)
                    {
                        //need to add create a foreign key
                        foreignKeys += string.Format(", FOREIGN KEY({0}) REFERENCES {1}(Id) ON DELETE CASCADE",
                            propertyInfo.Name, sqlProp.ReferenceTo.Name);
                    }
                }
                collumns += currentCollumn;
            }

            return string.Format(sql, name, collumns + foreignKeys);
        }

        public static string CreateInsert(Type type)
        {
            var props = type.GetRuntimeProperties()
                .Where(p => p.CustomAttributes.Count(n =>
                    n.AttributeType == typeof(SqlIgnore)) == 0 && (NetToSqlKepMap.ContainsKey(p.PropertyType) || p.PropertyType.GetTypeInfo().IsEnum));

            const string sql = "INSERT INTO {0} ({1}) VALUES ({2});";
            var name = type.Name;
            var propNames = "";
            var valueHolder = "";

            foreach (var propertyInfo in props)
            {
                var sqlProp = propertyInfo.GetCustomAttribute<SqlProperty>();
                if (sqlProp != null && sqlProp.IsPrimaryKey)
                    continue;

                var first = propNames == "";
                var prefix = (first ? "" : ", ");

                propNames += prefix + propertyInfo.Name;
                valueHolder += prefix + "?";
            }

            return string.Format(sql, name, propNames, valueHolder);
        }

        public static string CreateUpdate(Type type)
        {
            var props = type.GetRuntimeProperties()
                .Where(p => p.CustomAttributes.Count(n => 
                    n.AttributeType == typeof(SqlIgnore)) == 0 && (NetToSqlKepMap.ContainsKey(p.PropertyType) || p.PropertyType.GetTypeInfo().IsEnum));

            const string sql = "UPDATE {0} SET {1} WHERE Id = ?;";
            var name = type.Name;
            var propHolder = "";

            foreach (var propertyInfo in props)
            {
                if (propertyInfo.Name == "Id")
                    continue;
                var first = propHolder == "";
                var prefix = (first ? "" : ", ");

                propHolder += prefix + propertyInfo.Name + " = ?";
            }

            return string.Format(sql, name, propHolder);
        }

        public static void FillUpdate(ISQLiteStatement statement, BaseEntry obj)
        {
            FillStatement(statement, obj, true);
        }

        public static void FillInsert(ISQLiteStatement statement, BaseEntry obj)
        {
            FillStatement(statement, obj);
        }

        private static void FillStatement(ISQLiteStatement statement, BaseEntry obj, bool isUpdate = false)
        {
            var type = obj.GetType();

            var props = type.GetRuntimeProperties()
                .Where(p => p.CustomAttributes.Count(n =>
                    n.AttributeType == typeof(SqlIgnore)) == 0 && (NetToSqlKepMap.ContainsKey(p.PropertyType) || p.PropertyType.GetTypeInfo().IsEnum)).ToList();

            for (var i = 0; i < props.Count; i++)
            {
                var sqlProp = props[i].GetCustomAttribute<SqlProperty>();
                if (sqlProp != null && sqlProp.IsPrimaryKey)
                    continue;

                var proptype = props[i].PropertyType;

                //enums need to be cast as long
                object propvalue;

                if (proptype.GetTypeInfo().IsEnum)
                    propvalue = Convert.ToInt64(props[i].GetValue(obj));
                else if (proptype == typeof (DateTime))
                    propvalue = props[i].GetValue(obj).ToString();
                else if (proptype == typeof(TimeSpan))
                    propvalue = ((TimeSpan)props[i].GetValue(obj)).Ticks;
                else if (proptype == typeof(bool))
                    propvalue = Convert.ToInt32(props[i].GetValue(obj));
                else
                    propvalue = props[i].GetValue(obj);

                statement.Bind(i + 1, propvalue);
            }

            if (isUpdate)
                statement.Bind(props.Count, obj.Id);
        }
    }
}