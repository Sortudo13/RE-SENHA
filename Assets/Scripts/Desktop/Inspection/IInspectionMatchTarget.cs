public interface IInspectionMatchTarget
{
    bool TryRegisterInspectionMatch(
        InspectionField field,
        RegraData rule,
        SubregraData subrule
    );
}
