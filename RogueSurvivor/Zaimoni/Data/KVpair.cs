namespace Zaimoni.Data
{
    // for when the readonly value of the canonical KeyValuePair is problematic.
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
