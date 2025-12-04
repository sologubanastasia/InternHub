using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class IProjectMemberRepository
    {
        public void AddAsync(ProjectMember projectMember);
        public void RemoveAsync(ProjectMember projectMember);
        
    }

}    