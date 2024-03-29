﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;
using LentoCore.Expressions;

namespace LentoCore.Atoms
{
    public class Tuple : Atomic
    {
        public Expressions.Tuple BaseExpression;
        public Atomic[] Elements;
        public int Size => Elements?.Length ?? 0;

        public Tuple() : this(new Atomic[0]) { }
        public Tuple(params Atomic[] elements) : this(null, elements) { }
        public Tuple(Expressions.Tuple baseExpression, Atomic[] elements) : base(
            elements?.Length > 0
            ? new ObjectType($"{BaseType}<{elements?.Length ?? 0}>", elements?.Length ?? 0)
            : Unit.BaseType
            )
        {
            BaseExpression = baseExpression ?? new Expressions.Tuple(null, elements?.Select(e => (Expression) new AtomicValue<Atomic>(e, null)).ToArray());
            Elements = elements;
        }

        public new static AtomicType BaseType => new AtomicType(nameof(Tuple));
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => $"#({string.Join(", ", Elements.Select(e => e.ToString()))})";
    }
}
