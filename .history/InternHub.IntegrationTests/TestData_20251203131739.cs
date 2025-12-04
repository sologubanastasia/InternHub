using System;
using System.Collections.Generic;

namespace InternHub.IntegrationTests
{
    public static class TestData
    {
        // Кандидати
        public static readonly Guid OwnerCandidateId = new Guid("55555555-5555-5555-5555-555555555555");
        public static readonly Guid MemberCandidateId = new Guid("66666666-6666-6666-6666-666666666666");
        public static readonly Guid NonMemberCandidateId = new Guid("77777777-7777-7777-7777-777777777777");

        // Проекти
        public static readonly Guid ProjectIdForDetails = new Guid("44444444-4444-4444-4444-444444440001");
        public static readonly Guid ProjectIdForUpdate = new Guid("44444444-4444-4444-4444-444444440002");
        public static readonly Guid ProjectIdForMemberRemoval = new Guid("44444444-4444-4444-4444-444444440003");

        // Технології
        public static readonly Guid ExistingTech1Id = new Guid("99999999-9999-9999-9999-999999990001");
        public static readonly Guid ExistingTech2Id = new Guid("99999999-9999-9999-9999-999999990002");
        public static readonly Guid NonExistentTechId = new Guid("99999999-9999-9999-9999-FFFFFFFFFFFE");
    }
}