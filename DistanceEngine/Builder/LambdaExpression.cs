using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Distance.Engine.Builder
{
    public interface IExpression : IEquatable<IExpression>
    {
    }

    public class Identifier : IExpression
    {
        public string Name { get; }

        public Identifier(string name)
        {
            Name = name;
        }

        public bool Equals(IExpression other)
            => other is Identifier i && this.Name == i.Name;
    }

    public class Literal<T> : IExpression where T : IEquatable<T>
    {
        public T Value { get; }

        public Literal(T value)
        {
            Value = value;
        }

        public bool Equals(IExpression other)
            => other is Literal<T> l && this.Value.Equals(l.Value);
    }

    public class Call : IExpression
    {
        public IExpression Expr { get; }
        public ImmutableArray<IExpression> Arguments { get; }

        public Call(IExpression expr, ImmutableArray<IExpression> arguments)
        {
            Expr = expr;
            Arguments = arguments;
        }

        public bool Equals(IExpression other)
            => other is Call c
            && this.Expr.Equals(c.Expr)
            && this.Arguments.SequenceEqual(c.Arguments);
    }

    public enum UnaryOperatorType
    {
        Negate,
        Complement,
        Not
    }
    public class UnaryOp : IExpression
    {
        public UnaryOperatorType Type { get; }
        public IExpression Expr { get; }

        public UnaryOp(UnaryOperatorType type, IExpression expr)
        {
            Type = type;
            Expr = expr;
        }

        public bool Equals(IExpression other)
            => other is UnaryOp u
            && this.Type == u.Type
            && this.Expr.Equals(u.Expr);
    }

    public enum BinaryOperatorType
    {
        Add,
        Mul
    }
    public class BinaryOp : IExpression
    {
        public BinaryOperatorType Type { get; }
        public IExpression Left { get; }
        public IExpression Right { get; }

        public BinaryOp(BinaryOperatorType type, IExpression left, IExpression right)
        {
            Type = type;
            Left = left;
            Right = right;
        }

        public bool Equals(IExpression other)
            => other is BinaryOp b
            && this.Type == b.Type
            && this.Left.Equals(b.Left)
            && this.Right.Equals(b.Right);
    }
}
