using System;
using System.Collections.Immutable;
using System.Linq;

namespace Distance.Engine.Builder
{
    public abstract class Expression : IEquatable<Expression>
    {
        public abstract bool Equals(Expression other);
    }

    public class Identifier : Expression
    {
        public string Name { get; }

        public Identifier(string name)
        {
            Name = name;
        }

        public override bool Equals(Expression other)
            => other is Identifier i && this.Name == i.Name;
    }

    public class Literal<T> : Expression where T : IEquatable<T>
    {
        public T Value { get; }

        public Literal(T value)
        {
            Value = value;
        }

        public override bool Equals(Expression other)
            => other is Literal<T> l && this.Value.Equals(l.Value);
    }

    public class Call : Expression
    {
        public Expression Expr { get; }
        public ImmutableArray<Expression> Arguments { get; }

        public Call(Expression expr, ImmutableArray<Expression> arguments)
        {
            Expr = expr;
            Arguments = arguments;
        }

        public override bool Equals(Expression other)
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
    public class UnaryOp : Expression
    {
        public UnaryOperatorType Type { get; }
        public Expression Expr { get; }

        public UnaryOp(UnaryOperatorType type, Expression expr)
        {
            Type = type;
            Expr = expr;
        }

        public override bool Equals(Expression other)
            => other is UnaryOp u
            && this.Type == u.Type
            && this.Expr.Equals(u.Expr);
    }

    public enum BinaryOperatorType
    {
        Add,
        Mul
    }
    public class BinaryOp : Expression
    {
        public BinaryOperatorType Type { get; }
        public Expression Left { get; }
        public Expression Right { get; }

        public BinaryOp(BinaryOperatorType type, Expression left, Expression right)
        {
            Type = type;
            Left = left;
            Right = right;
        }

        public override bool Equals(Expression other)
            => other is BinaryOp b
            && this.Type == b.Type
            && this.Left.Equals(b.Left)
            && this.Right.Equals(b.Right);
    }
}
