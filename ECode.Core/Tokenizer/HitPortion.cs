
namespace ECode.Tokenizer
{
    public sealed class HitPortion
    {
        public int Offset
        { get; private set; }

        public int Length
        { get; set; }

        public PhrasePortion Portion
        { get; set; }


        public HitPortion(int offset, PhrasePortion portion)
        {
            this.Offset = offset;
            this.Length = portion.Portion.Length;
            this.Portion = portion;
        }
    }
}