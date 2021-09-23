namespace LentoCore.Atoms.Types
{
    public class AtomicType : Atomic
    {
        public string Name;
        private AtomicType() : base(null) { }
        public AtomicType(string name) : this()
        {
            Name = name;
            Type = BaseType;
        }

        public virtual bool Equals(AtomicType other)
        {
            if (Name == other.Name) return true;
            if (this is AnyType || other is AnyType) return true;
            if (other.Name == "Type" && Name != "Type" && BaseType.Name == "Type") return true;
            if (other.Name != "Type" && Name == "Type" && other.Type.Name == "Type") return true;
            return false;
        }
        public new static AtomicType BaseType => GetBaseType(); // Base type

        private static AtomicType GetBaseType()
        {
            AtomicType t1 = new AtomicType
            {
                Name = "Type"
            };
            AtomicType t2 = new AtomicType
            {
                Name = "Type",
                Type = t1
            };
            t1.Type = t2;
            return t1;
        }
        
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Name;
    }
}
