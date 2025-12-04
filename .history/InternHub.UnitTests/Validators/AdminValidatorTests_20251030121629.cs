namespace InternHub.UnitTests.Validators;

using InternHub.Infrastructure.Repositories;
using InternHub.Application.Validators;
using InternHub.Application.DTO.Admin;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using InternHub.Domain.Entities;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using Moq;

public class AdminServiceTests
{
    private readonly Mock<IAuthRepository> _repo
}