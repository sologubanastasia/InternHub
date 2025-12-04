using System;
using System.Collections.Generic;

namespace InternHub.IntegrationTests
{
    public static class TestData
    {
        // ID користувачів, які визначені у вашому SeedTestDataAsync
        public static readonly Guid AdminId = new Guid("11111111-1111-1111-1111-111111111111");
        public static readonly Guid CompanyId = new Guid("22222222-2222-2222-2222-222222222222");
        public static readonly Guid OwnerCandidateId = new Guid("33333333-3333-3333-3333-333333333333"); // Власник тестових проектів
        public static readonly Guid NonExistentUserId = new Guid("99999999-9999-9999-9999-999999999999");
        
        // Додатковий член проекту (посів)
        public static readonly Guid MemberCandidateId = new Guid("44444444-4444-4444-4444-444444444444");
        public static readonly Guid NonMemberCandidateId = new Guid("55555555-5555-5555-5555-555555555555");
        
        // ID Проектів
        public static readonly Guid ProjectIdForDetails = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public static readonly Guid ProjectIdForUpdate = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public static readonly Guid ProjectIdForMemberRemoval = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");
        public static readonly Guid NonExistentProjectId = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

        // ID Технологій
        public static readonly Guid ExistingTech1Id = new Guid("f0000001-f000-f000-f000-f00000000001");
        public static readonly Guid ExistingTech2Id = new Guid("f0000002-f000-f000-f000-f00000000002");
        public static readonly Guid NonExistentTechId = new Guid("f0000003-f000-f000-f000-f00000000003");
    }
}