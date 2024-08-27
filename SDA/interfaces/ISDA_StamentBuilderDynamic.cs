using SIPS.Framework.SDA.Api;

namespace SIPS.Framework.SDA.interfaces
{
    public interface ISDA_StamentBuilderDynamic : ISDA_DynamicExtension
    {
        string BuildQuery(SDA_DataSourceDefinition dataSourceDefinition);
    }
}


