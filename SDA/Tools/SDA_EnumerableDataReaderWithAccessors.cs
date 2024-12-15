using System;
using System.Collections.Generic;
using System.Data;

namespace SIPS.Framework.SDA.Tools
{
    public class SDA_EnumerableDataReaderWithAccessors<T> : IDataReader
    {
        private IEnumerator<T> _enumerator;
        private List<Func<T, object>> _accessors;
        private List<string> _fieldNames;
        private int _currentIndex = 0;

        public SDA_EnumerableDataReaderWithAccessors(IEnumerable<T> enumerable, List<Func<T, object>> accessors, List<string> fieldNames)
        {
            _enumerator = enumerable.GetEnumerator();
            _accessors = accessors;
            _fieldNames = fieldNames;
        }

        public object GetValue(int i) => _accessors[i](_enumerator.Current);
        public bool Read()
        {
            if (_enumerator.MoveNext())
            {
                _currentIndex++;
                return true;
            }
            return false;
        }
        public int FieldCount => _accessors.Count;
        public string GetName(int i) => _fieldNames[i];
        public void Dispose() => _enumerator.Dispose();

        // Other IDataReader methods can be implemented as needed
        public bool IsDBNull(int i) => GetValue(i) == null;
        public int GetOrdinal(string name) => _fieldNames.IndexOf(name);
        public Type GetFieldType(int i) => GetValue(i)?.GetType() ?? typeof(object);
        public string GetDataTypeName(int i) => GetFieldType(i).Name;
        public int GetValues(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = GetValue(i);
            }
            return values.Length;
        }

        // Not implemented methods
        public bool NextResult() => throw new NotImplementedException();
        public void Close() => throw new NotImplementedException();
        public DataTable GetSchemaTable() => throw new NotImplementedException();
        public int Depth => throw new NotImplementedException();
        public bool IsClosed => throw new NotImplementedException();
        public int RecordsAffected => throw new NotImplementedException();
        public bool GetBoolean(int i) => throw new NotImplementedException();
        public byte GetByte(int i) => throw new NotImplementedException();
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotImplementedException();
        public char GetChar(int i) => throw new NotImplementedException();
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotImplementedException();
        public IDataReader GetData(int i) => throw new NotImplementedException();
        public DateTime GetDateTime(int i) => throw new NotImplementedException();
        public decimal GetDecimal(int i) => throw new NotImplementedException();
        public double GetDouble(int i) => throw new NotImplementedException();
        public float GetFloat(int i) => throw new NotImplementedException();
        public Guid GetGuid(int i) => throw new NotImplementedException();
        public short GetInt16(int i) => throw new NotImplementedException();
        public int GetInt32(int i) => throw new NotImplementedException();
        public long GetInt64(int i) => throw new NotImplementedException();
        public string GetString(int i) => throw new NotImplementedException();
        public object this[int i] => throw new NotImplementedException();
        public object this[string name] => throw new NotImplementedException();
    }


}
