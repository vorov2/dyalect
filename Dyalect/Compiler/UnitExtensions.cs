namespace Dyalect.Compiler
{
    internal static class RuntimeContextExtensions
    {
        public static int GetMemberId(this Unit unit, string name)
        {
            if (!unit.MemberNames.TryGetValue(name, out var id))
            {
                id = unit.MemberIds.Count;
                unit.MemberIds.Add(-1);
                unit.MemberNames.Add(name, id);
            }

            return id;
        }

        public static int GetMemberId(this UnitComposition composition, string memberName)
        {
            if (!composition.MembersMap.TryGetValue(memberName, out var id))
            {
                id = composition.Members.Count;
                composition.Members.Add(memberName);
                composition.MembersMap.Add(memberName, id);
            }

            return id;
        }
    }
}
