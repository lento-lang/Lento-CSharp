﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public abstract class Atomic
    {
        public abstract Atoms.AtomicType GetAtomicType();
        // public abstract string GetTypeName();
        public new abstract string ToString();
    }
}
