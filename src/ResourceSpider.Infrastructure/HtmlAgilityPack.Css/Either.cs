namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

internal abstract class Either<TA, TB>
    : IEquatable<Either<TA, TB>>
{
    private Either() { }

    public static Either<TA, TB> A(TA value)
    {
        if (value == null) throw new ArgumentNullException("value");
        return new AImpl(value);
    }

    public static Either<TA, TB> B(TB value)
    {
        if (value == null) throw new ArgumentNullException("value");
        return new BImpl(value);
    }

    public abstract override bool Equals(object? obj);
    public abstract bool Equals(Either<TA, TB>? obj);
    public abstract override int GetHashCode();
    public abstract override string ToString();
    public abstract TResult Fold<TResult>(Func<TA, TResult> a, Func<TB, TResult> b);

    private sealed class AImpl(TA value) : Either<TA, TB>
    {
        private readonly TA _value = value;

        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        public override bool Equals(object? obj) => Equals(obj as AImpl);
        public override bool Equals(Either<TA, TB>? obj) => obj is AImpl a && EqualityComparer<TA>.Default.Equals(_value!, a._value);
        public override TResult Fold<TResult>(Func<TA, TResult> a, Func<TB, TResult> b)
        {
            if (a == null) throw new ArgumentNullException("a");
            if (b == null) throw new ArgumentNullException("b");
            return a(_value!);
        }
        public override string ToString() => _value?.ToString() ?? string.Empty;
    }

    private sealed class BImpl(TB value) : Either<TA, TB>
    {
        private readonly TB _value = value;

        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        public override bool Equals(object? obj) => Equals(obj as BImpl);
        public override bool Equals(Either<TA, TB>? obj) => obj is BImpl b && EqualityComparer<TB>.Default.Equals(_value!, b._value);
        public override TResult Fold<TResult>(Func<TA, TResult> a, Func<TB, TResult> b)
        {
            if (a == null) throw new ArgumentNullException("a");
            if (b == null) throw new ArgumentNullException("b");
            return b(_value!);
        }
        public override string ToString() => _value?.ToString() ?? string.Empty;
    }
}
