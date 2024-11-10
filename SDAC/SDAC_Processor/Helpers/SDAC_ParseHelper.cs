using System;
using System.Data;

namespace SIPS.Framework.SDAC_Processor.Helpers
{
    // create un metodo estensione per la classe SDAC_OperationDefinition_inline_data_source per generare un oggetto SDA_DataSourceDefinition
    // restituendo un oggetto sdac_response con value di tipo SDA_DataSourceDefinition e success = true
    // prendendo in inputr un oggett det di tipo sdac_etlsourceDefinition

    public static class SDAC_ParseHelper
    {

        public static DbType? GetDBType(string data_type)
        {
            switch (data_type)
            {
                case "string":
                    return DbType.String;
                case "int":
                case "int32":
                    return DbType.Int32;
                case "long":
                case "int64":
                    return DbType.Int64;
                case "double":
                    return DbType.Double;
                case "float":
                    return DbType.Single;
                case "decimal":
                    return DbType.Decimal;
                case "bool":
                case "boolean":
                    return DbType.Boolean;
                case "datetime":
                    return DbType.DateTime;
                case "timespan":
                    return DbType.Time;
                default:
                    return null;   // fallback to string
            }
        }

        public static object ParseValue(string value, string type)
        {
            switch (type)
            {
                case "string":
                    return value;
                case "int":
                case "int32":
                    return int.Parse(value);
                case "long":
                case "int64":
                    return long.Parse(value);
                case "double":
                    return double.Parse(value);
                case "float":
                    return float.Parse(value);
                case "decimal":
                    return decimal.Parse(value);
                case "bool":
                case "boolean":
                    return bool.Parse(value);
                case "datetime":
                    return DateTime.Parse(value);
                case "timespan":
                    return TimeSpan.Parse(value);
                default:
                    return value;   // fallback to string
            }
        }

        public static object ParseValueByBDType(string value, DbType? dbType)
        {
            switch (dbType)
            {

                case DbType.String:
                    return value;
                case DbType.Int32:
                    return int.Parse(value);
                case DbType.Int64:
                    return long.Parse(value);
                case DbType.Double:
                    return double.Parse(value);
                case DbType.Single:
                    return float.Parse(value);
                case DbType.Decimal:
                    return decimal.Parse(value);
                case DbType.Boolean:
                    return bool.Parse(value);
                case DbType.DateTime:
                    return DateTime.Parse(value);
                case DbType.Time:
                    return TimeSpan.Parse(value);
                default:
                    return value;   // fallback to string

            }
        }
    }

}
