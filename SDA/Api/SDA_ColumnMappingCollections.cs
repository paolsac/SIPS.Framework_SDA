using System.Collections.Generic;

namespace SIPS.Framework.SDA.Api
{
    public class SDA_ColumnMappingCollections
    {
        // create enum for column mapping type
        public enum ColumnMappingType
        {
            nameToName,
            indexToIndex,
            nameToIndex,
            indexToName
        }

        private readonly ColumnMappingType _mappingType;
        private readonly List<SDA_DBColumnMapping> _columnMappings;

        public ColumnMappingType MappingType => _mappingType;

        public SDA_ColumnMappingCollections(ColumnMappingType mappingType)
        {
            _mappingType = mappingType;
            _columnMappings = new List<SDA_DBColumnMapping>();
        }

        public void AddMapping(SDA_DBColumnMapping mapping)
        {
            _columnMappings.Add(mapping);
        }

        // add method to set multiple mappings
        public void AddMappings(IEnumerable<SDA_DBColumnMapping> mappings)
        {
            _columnMappings.AddRange(mappings);
        }

        // add method to get all mappings as read only
        public IReadOnlyList<SDA_DBColumnMapping> GetMappings()
        {
            return _columnMappings.AsReadOnly();
        }
    }

}
