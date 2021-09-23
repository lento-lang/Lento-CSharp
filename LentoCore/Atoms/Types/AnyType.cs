namespace LentoCore.Atoms.Types
{
    public class AnyType : AtomicType
    {
        public AnyType() : base("any") { }
        public new static AtomicType BaseType => new AnyType();
    }
}
