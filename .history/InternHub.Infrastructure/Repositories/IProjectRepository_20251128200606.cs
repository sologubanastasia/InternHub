using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class  IProjectRepository
    {
        public void AddAsync(Project project);
        public void RemoveAsync(Project project);
    }

}    