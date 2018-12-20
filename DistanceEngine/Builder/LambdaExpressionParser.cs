    using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Pidgin;
using Pidgin.Expression;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Distance.Engine.Builder
{
    public static class ExprParser
    {
        private static Parser<char, T> Token<T>(Parser<char, T> token)
            => Try(token).Before(SkipWhitespaces);

        private static Parser<char, string> Token(string token)
            => Token(String(token));

        private static Parser<char, T> Parenthesised<T>(Parser<char, T> parser)
            => parser.Between(Token("("), Token(")"));

        public static Parser<char, string> StrConstant 
            => AnyCharExcept('"').ManyString().Between(Token("\""), Token("\""));

        private static Parser<char, Func<IExpression, IExpression, IExpression>> Binary(Parser<char, BinaryOperatorType> op)
            => op.Select<Func<IExpression, IExpression, IExpression>>(type => (l, r) => new BinaryOp(type, l, r));

        private static Parser<char, Func<IExpression, IExpression>> Unary(Parser<char, UnaryOperatorType> op)
            => op.Select<Func<IExpression, IExpression>>(type => o => new UnaryOp(type, o));

        #region Binary Artihmetic Operations
        private static readonly Parser<char, Func<IExpression, IExpression, IExpression>> Add
            = Binary(Token("+").ThenReturn(BinaryOperatorType.Add));

        private static readonly Parser<char, Func<IExpression, IExpression, IExpression>> Subtract
            = Binary(Token("-").ThenReturn(BinaryOperatorType.Add));

        private static readonly Parser<char, Func<IExpression, IExpression, IExpression>> Multiply
            = Binary(Token("*").ThenReturn(BinaryOperatorType.Mul));

        private static readonly Parser<char, Func<IExpression, IExpression, IExpression>> Divide
            = Binary(Token("/").ThenReturn(BinaryOperatorType.Mul));

        private static readonly Parser<char, Func<IExpression, IExpression, IExpression>> Modulo
            = Binary(Token("%").ThenReturn(BinaryOperatorType.Mul));

        #endregion
        #region Bitwise Operations
        // And         &
        // Or          |
        // ExclusiveOr ^

        #endregion
        #region Shift Operations
        // LetShift <<
        // RightShift >>
        #endregion
        #region Conditional Boolean Operations
        // AndAlso &&
        // OrElse  ||
        #endregion
        #region Comparison Operations
        // Equal
        // NotEqual
        // GreaterThanOrEqual
        // GreaterThan
        // LessThan
        // LessThanOrEqual
        #endregion

        #region Unary Operators
        private static readonly Parser<char, Func<IExpression, IExpression>> Negate
            = Unary(Token("-").ThenReturn(UnaryOperatorType.Negate));

        private static readonly Parser<char, Func<IExpression, IExpression>> Complement
            = Unary(Token("~").ThenReturn(UnaryOperatorType.Complement));

        private static readonly Parser<char, Func<IExpression, IExpression>> Not
            = Unary(Token("!").ThenReturn(UnaryOperatorType.Not));

        #endregion

        private static readonly Parser<char, IExpression> Identifier
            = Token(Letter.Then(LetterOrDigit.Or(Char('.')).ManyString(), (h, t) => h + t))
                .Select<IExpression>(name => new Identifier(name))
                .Labelled("identifier");

        private static readonly Parser<char, IExpression> IntegerLiteral
            = Token(Num)
                .Select<IExpression>(value => new Literal<int>(value))
                .Labelled("integer literal");

        private static readonly Parser<char, IExpression> StringLiteral
            = Token(StrConstant)
                .Select<IExpression>(value => new Literal<string>(value))
                .Labelled("string literal");


        private static Parser<char, IExpression> BuildExpressionParser()
        {
            Parser<char, IExpression> expr = null;

            var term = OneOf(
                Identifier,
                IntegerLiteral,
                Parenthesised(Rec(() => expr)).Labelled("parenthesised expression")
            );

            var call = Parenthesised(Rec(() => expr).Separated(Token(",")))
                .Select<Func<IExpression, IExpression>>(args => method => new Call(method, args.ToImmutableArray()))
                .Labelled("function call");

            expr = ExpressionParser.Build(
                term,
                new[]
                {
                    Operator.PostfixChainable(call),
                    Operator.Prefix(Negate).And(Operator.Prefix(Complement)),
                    Operator.InfixL(Multiply),
                    Operator.InfixL(Add)
                }
            ).Labelled("expression");

            return expr;
        }

        private static readonly Parser<char, IExpression> Expr = BuildExpressionParser();

        

        public static IExpression ParseOrThrow(string input)
            => Expr.ParseOrThrow(input);
    }
}
