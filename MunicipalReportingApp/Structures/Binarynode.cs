namespace MunicipalReportingApp.Structures
{
    
  
    public sealed class BinaryNode<TKey, TValue>
    {
        public BinaryNode(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public BinaryNode<TKey, TValue>? Left { get; set; }
        public BinaryNode<TKey, TValue>? Right { get; set; }

       
        public int Height { get; set; } = 1;

        
        public bool IsRed { get; set; }
    }
}