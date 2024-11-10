using Dapper;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDAC_Processor.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace SIPS.Framework.SDAC_Processor.Extensions
{
    public static class SDA_DataSourceDefinitionExtensions
    {
        public static void OverrideParameters(this SDA_DataSourceDefinition datasource, Dictionary<string, object> externalParameters)
        {
            if (externalParameters != null && externalParameters.Any())
            {
                // se il datasource ha già dei parametri, li sovrascrive con quelli passati
                if (datasource.ParametersGetter != null)
                {

                    var ups = datasource.ParametersGetter();
                    if (ups is Dictionary<string, object>)
                    {
                        var ps = ups as Dictionary<string, object>;
                        foreach (var param in externalParameters)
                        {
                            if (ps.ContainsKey(param.Key))
                            {
                                ps[param.Key] = ParseAsOriginalTypeIfString(param.Value, ps[param.Key].GetType());
                            }
                            else
                            {
                                ps.Add(param.Key, param.Value);
                            }
                        }
                    }
                    else if (ups is DynamicParameters)
                    {
                        var ps = ups as DynamicParameters;
                        var iLookup = (SqlMapper.IParameterLookup)ps;
                        var t = ps.GetType().GetField("parameters", BindingFlags.NonPublic | BindingFlags.Instance);

                        foreach (var item in externalParameters)
                        {
                            if (ps.ParameterNames.Contains(item.Key))
                            {
                                // se item.value è una stringa, prova a fare il parse in base al tipo originale o al dbtype, con priorità al dbtype
                                if (item.Value is string)
                                {
                                    // provo con il dbtype
                                    if (t != null)
                                    {
                                        object a  = ((IDictionary)t.GetValue(ps))[item.Key];
                                        
                                        DbType? dbType = (DbType)a?.GetType().GetProperty("DbType")?.GetValue(a);
                                        if (dbType != null)
                                        {
                                            ps.Add(item.Key, SDAC_ParseHelper.ParseValueByBDType(item.Value.ToString(), dbType), dbType);
                                            continue;
                                        }
                                    }
                                    // provo con il tipo originale
                                    var origValue = iLookup[item.Key];
                                    if (origValue != null)
                                    {
                                        ps.Add(item.Key, ParseAsOriginalTypeIfString(item.Value, origValue.GetType()));
                                    }
                                    else
                                    {
                                        ps.Add(item.Key, item.Value);
                                    }
                                }
                            }
                            else
                            {
                                ps.Add(item.Key, item.Value);
                            }
                        }
                    }
                    else
                    {
                        throw new System.Exception("Unsupported parameter type");
                    }

                }

                // se il datasource non ha parametri, li passa direttamente
                else
                {
                    datasource.ParametersGetter = () => externalParameters;
                }
            }
        }

        private static object ParseAsOriginalTypeIfString(object v, Type type)
        {
            if (!(v is string))
            {
                return v;
            }
            switch (type)
            {
                case Type t when t == typeof(int):
                    return int.Parse(v.ToString());
                case Type t when t == typeof(long):
                    return long.Parse(v.ToString());
                case Type t when t == typeof(double):
                    return double.Parse(v.ToString());
                case Type t when t == typeof(DateTime):
                    return DateTime.Parse(v.ToString());
                case Type t when t == typeof(TimeSpan):
                    return TimeSpan.Parse(v.ToString());
                case Type t when t == typeof(bool):
                    return bool.Parse(v.ToString());
                case Type t when t == typeof(float):
                    return float.Parse(v.ToString());
                case Type t when t == typeof(decimal):
                    return decimal.Parse(v.ToString());

                default:
                    return v.ToString();
            }
        }
    }
}

