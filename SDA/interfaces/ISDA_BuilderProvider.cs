using SIPS.Framework.SDA.Api;

namespace SIPS.Framework.SDA.interfaces
{
    internal interface ISDA_BuilderProvider
    {
        string BuildQuery(SDA_DataSourceDefinition dataSourceDefinition, bool applyPlaceholders = true);
    }
}