namespace Ampere
{
    struct Either<TLeft, TRight>
    {
        public bool IsLeft { get; }
        public bool IsRight => !IsLeft;
        public TLeft Left { get; }
        public TRight Right { get; }
        public Either(TLeft left)
        {
            IsLeft = true;
            Left = left;
            Right = default(TRight);
        }
        public Either(TRight right)
        {
            IsLeft = false;
            Left = default(TLeft);
            Right = right;
        }
    }
}
