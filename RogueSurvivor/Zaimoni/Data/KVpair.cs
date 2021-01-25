namespace Zaimoni.Data
{
    // for when the readonly value of the canonical KeyValuePair is problematic.
    // 2021-01-24: struct version cannot assign to Value reliably after construction
    public class KVpair<K,V>
    {
        public readonly K Key;
        public V Value;

        public KVpair(K k, V v) {
            Key = k;
            Value = v;
        }
    }
}
