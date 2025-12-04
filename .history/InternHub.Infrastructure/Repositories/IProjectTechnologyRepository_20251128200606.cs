using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class IProjectTechnologyRepository
    {
        public void AddAsync(ProjectTechnology projectTechnology);
        public void RemoveAsync(ProjectTechnology projectTechnology);
    }

}    