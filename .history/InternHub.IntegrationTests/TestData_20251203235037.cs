using System;

namespace InternHub.IntegrationTests
{
    public static class TestData
    {
        public static readonly Guid OwnerCandidateId = new Guid("10000000-0000-0000-0000-000000000001");
        public static readonly Guid MemberCandidateId = new Guid("10000000-0000-0000-0000-000000000002");
        public static readonly Guid NonMemberCandidateId = new Guid("10000000-0000-0000-0000-000000000003");
        public static readonly Guid ExistingCompanyId = new Guid("20000000-0000-0000-0000-000000000001");
        
        public static readonly Guid ExistingTech1Id = new Guid("30000000-0000-0000-0000-000000000001");
        public static readonly Guid ExistingTech2Id = new Guid("30000000-0000-0000-0000-000000000002");
        public static readonly Guid NonExistentTechId = new Guid("30000000-0000-0000-0000-000000000009");
        
        public static readonly Guid ProjectIdForDetails = new Guid("40000000-0000-0000-0000-000000000001");
        public static readonly Guid ProjectIdForUpdate = new Guid("40000000-0000-0000-0000-000000000002");
        public static readonly Guid ProjectIdForMemberRemoval = new Guid("40000000-0000-0000-0000-000000000003");
        
        public static readonly Guid ExistingTeamId = new Guid("50000000-0000-0000-0000-000000000001");
    }
}