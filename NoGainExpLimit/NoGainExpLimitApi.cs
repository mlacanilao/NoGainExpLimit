namespace NoGainExpLimit;

public static class NoGainExpLimitApi
{
    public static bool IsModExpContinuationCall
    {
        get
        {
            return ElementContainerPatch.IsModExpContinuationCall;
        }
    }
}
