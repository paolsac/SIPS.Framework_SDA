
namespace SIPS.Framework.SDA.interfaces
{
    internal interface ISDA_SupportExtensions<T>
    {
        bool AddExtension(ISDA_StamentBuilderDynamic extension, bool rejectReplace = false, bool exceptionIfReplaced = true);
    }
}