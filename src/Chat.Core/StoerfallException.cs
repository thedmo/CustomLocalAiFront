namespace Chat.Core;

public sealed class StoerfallException(Stoerfall fall, string grund) : Exception(grund)
{
    public Stoerfall Fall { get; } = fall;
}
