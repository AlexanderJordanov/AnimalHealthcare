using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.GCommon.Enums;
using AnimalHealthcare.Services.Core;
using AnimalHealthcare.Web.ViewModels.UserProfile;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;


namespace AnimalHealthcare.Tests.Services
{
    [TestFixture]
    public class UserProfileServiceTests
    {
        private Mock<UserManager<IdentityUser>> _userManagerMock = null!;
        private Mock<SignInManager<IdentityUser>> _signInManagerMock = null!;
        private IdentityUser _identityUser1 = null!;
        private IdentityUser _identityUser2 = null!;
        private AnimalHealthcareDbContext _context = null!;
        private UserProfileService _service = null!;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<AnimalHealthcareDbContext>()
                .UseInMemoryDatabase("UserProfileServiceTestDb_" + Guid.NewGuid())
                .Options;

            _context = new AnimalHealthcareDbContext(options);

            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Mock SignInManager dependencies
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());
            var loggerMock = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemeProviderMock = new Mock<IAuthenticationSchemeProvider>();

            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                contextAccessorMock.Object,
                userPrincipalFactoryMock.Object,
                optionsMock.Object,
                loggerMock.Object,
                schemeProviderMock.Object
            );

            // Setup mock methods for SignInManager
            _signInManagerMock.Setup(sm => sm.RefreshSignInAsync(It.IsAny<IdentityUser>()))
                .Returns(Task.CompletedTask);
            _signInManagerMock.Setup(sm => sm.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Create Identity Users
            _identityUser1 = new IdentityUser
            {
                Id = "user-1",
                Email = "user1@example.com",
                UserName = "user1@example.com"
            };

            _identityUser2 = new IdentityUser
            {
                Id = "user-2",
                Email = "user2@example.com",
                UserName = "user2@example.com"
            };

            // Seed Identity Users in DbContext.Users
            _context.Users.AddRange(_identityUser1, _identityUser2);

            // Seed UserProfiles linked to Identity users
            var profile1 = new UserProfile
            {
                Id = "user-1",
                FullName = "User One",
                PhoneNumber = "1234567890",
                Address = "123 Main St",
                ProfilePictureUrl = "pic1.jpg",
                User = _identityUser1
            };

            var profile2 = new UserProfile
            {
                Id = "user-2",
                FullName = "User Two",
                PhoneNumber = "0987654321",
                Address = "456 Elm St",
                ProfilePictureUrl = "pic2.jpg",
                User = _identityUser2
            };

            _context.UserProfiles.AddRange(profile1, profile2);

            // Seed Animals linked to profile1
            var animal1 = new Animal
            {
                Id = 1,
                Name = "Buddy",
                Species = "Dog",
                Breed = "Labrador",
                Age = 5,
                Gender = AnimalGender.Male,
                UserProfileId = "user-1",
                IsDeleted = false
            };

            var animal2 = new Animal
            {
                Id = 2,
                Name = "Milo",
                Species = "Cat",
                Breed = "Siamese",
                Age = 3,
                Gender = AnimalGender.Female,
                UserProfileId = "user-1",
                IsDeleted = true // Deleted animal should be excluded
            };

            _context.Animals.AddRange(animal1, animal2);

            // Seed Appointments linked to animal1 with UserProfileId set
            var appointment1 = new Appointment
            {
                Id = 1,
                AnimalId = 1,
                UserProfileId = "user-1",
                AppointmentDateTime = DateTime.UtcNow.AddDays(1),
                IsDeleted = false
            };

            var appointment2 = new Appointment
            {
                Id = 2,
                AnimalId = 1,
                UserProfileId = "user-1",
                AppointmentDateTime = DateTime.UtcNow.AddDays(2),
                IsDeleted = true
            };

            _context.Appointments.AddRange(appointment1, appointment2);

            await _context.SaveChangesAsync();

            // Setup UserManager mock behaviors
            _userManagerMock.Setup(um => um.FindByIdAsync("user-1")).ReturnsAsync(_identityUser1);
            _userManagerMock.Setup(um => um.FindByIdAsync("user-2")).ReturnsAsync(_identityUser2);
            _userManagerMock.Setup(um => um.FindByIdAsync(It.Is<string>(id => id != "user-1" && id != "user-2"))).ReturnsAsync((IdentityUser?)null);

            _userManagerMock.Setup(um => um.GenerateChangeEmailTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync("token");

            _userManagerMock.Setup(um => um.ChangeEmailAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), "token"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(um => um.DeleteAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Instantiate service with mocks
            _service = new UserProfileService(_context, _userManagerMock.Object, _signInManagerMock.Object);
        }



        [TearDown]
        public async Task TearDown()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        [Test]
        public async Task CreateUserProfileAsync_AddsProfile()
        {
            var newUserId = "user-3";
            var fullName = "User Three";

            await _service.CreateUserProfileAsync(newUserId, fullName, "111222333", "789 Oak St", "pic3.jpg");

            var profile = await _context.UserProfiles.FindAsync(newUserId);
            Assert.IsNotNull(profile);
            Assert.That(profile.FullName, Is.EqualTo(fullName));
            Assert.That(profile.PhoneNumber, Is.EqualTo("111222333"));
            Assert.That(profile.Address, Is.EqualTo("789 Oak St"));
            Assert.That(profile.ProfilePictureUrl, Is.EqualTo("pic3.jpg"));
        }

        [Test]
        public async Task GetByEmailAsync_UserNotFound_ReturnsNull()
        {
            var result = await _service.GetByEmailAsync("nonexistent@example.com");
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetByEmailAsync_UserFound_ReturnsProfile()
        {
            var result = await _service.GetByEmailAsync("user1@example.com");
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo("user-1"));
        }

        [Test]
        public async Task GetProfileByIdAsync_Unauthorized_ReturnsNull()
        {
            var result = await _service.GetProfileByIdAsync("user-1", "user-2");
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetProfileByIdAsync_NotFound_ReturnsNull()
        {
            var result = await _service.GetProfileByIdAsync("user-99", "user-99");
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetProfileByIdAsync_Authorized_ReturnsProfile()
        {
            var result = await _service.GetProfileByIdAsync("user-1", "user-1");
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo("user-1"));
            Assert.That(result.User.Email, Is.EqualTo("user1@example.com"));
        }

        [Test]
        public void BuildUserProfileViewModel_MapsCorrectly()
        {
            var profile = _context.UserProfiles.First();
            var animals = new List<AnimalSummaryViewModel>
            {
                new AnimalSummaryViewModel
                {
                    Id = 1,
                    Name = "Buddy",
                    Species = "Dog",
                    Breed = "Labrador"
                }
            };

            var vm = _service.BuildUserProfileViewModel(profile, animals);

            Assert.That(vm.Email, Is.EqualTo(profile.User.Email));
            Assert.That(vm.FullName, Is.EqualTo(profile.FullName));
            Assert.That(vm.PhoneNumber, Is.EqualTo(profile.PhoneNumber));
            Assert.That(vm.Address, Is.EqualTo(profile.Address));
            Assert.That(vm.ProfilePictureUrl, Is.EqualTo(profile.ProfilePictureUrl));
            Assert.That(vm.Animals, Is.EqualTo(animals));
        }

        [Test]
        public async Task UpdateProfilePictureAsync_Unauthorized_ReturnsUnauthorized()
        {
            var result = await _service.UpdateProfilePictureAsync("user-1", "newpic.jpg", "user-2");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Unauthorized));
        }

        [Test]
        public async Task UpdateProfilePictureAsync_ProfileNotFound_ReturnsNotFound()
        {
            var result = await _service.UpdateProfilePictureAsync("user-99", "newpic.jpg", "user-99");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NotFound));
        }

        [Test]
        public async Task UpdateProfilePictureAsync_NoChange_ReturnsNoChange()
        {
            var profile = await _context.UserProfiles.FindAsync("user-1");
            var result = await _service.UpdateProfilePictureAsync("user-1", profile.ProfilePictureUrl, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NoChange));
        }

        [Test]
        public async Task UpdateProfilePictureAsync_UpdatesSuccessfully()
        {
            var result = await _service.UpdateProfilePictureAsync("user-1", "updatedpic.jpg", "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Success));
            var profile = await _context.UserProfiles.FindAsync("user-1");
            Assert.That(profile.ProfilePictureUrl, Is.EqualTo("updatedpic.jpg"));
        }

        [Test]
        public async Task BuildEditEmailViewModelAsync_Unauthorized_ReturnsNull()
        {
            var result = await _service.BuildEditEmailViewModelAsync("user-1", "user-2");
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildEditEmailViewModelAsync_ProfileNotFound_ReturnsNull()
        {
            var result = await _service.BuildEditEmailViewModelAsync("user-99", "user-99");
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildEditEmailViewModelAsync_ReturnsEmail()
        {
            var result = await _service.BuildEditEmailViewModelAsync("user-1", "user-1");
            Assert.IsNotNull(result);
            Assert.That(result.Email, Is.EqualTo("user1@example.com"));
        }

        [Test]
        public async Task UpdateEmailAsync_Unauthorized_ReturnsUnauthorized()
        {
            var model = new EditEmailViewModel { Email = "newemail@example.com" };
            var result = await _service.UpdateEmailAsync("user-1", model, "user-2");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Unauthorized));
        }

        [Test]
        public async Task UpdateEmailAsync_UserNotFound_ReturnsNotFound()
        {
            var model = new EditEmailViewModel { Email = "newemail@example.com" };
            var result = await _service.UpdateEmailAsync("user-99", model, "user-99");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NotFound));
        }

        [Test]
        public async Task UpdateEmailAsync_NoChange_ReturnsNoChange()
        {
            var model = new EditEmailViewModel { Email = "user1@example.com" };
            var result = await _service.UpdateEmailAsync("user-1", model, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NoChange));
        }

        [Test]
        public async Task UpdateEmailAsync_FailureChangingEmail_ReturnsFailed()
        {
            var model = new EditEmailViewModel { Email = "newemail@example.com" };
            _userManagerMock.Setup(um => um.ChangeEmailAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());
            var result = await _service.UpdateEmailAsync("user-1", model, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Failed));
        }

        [Test]
        public async Task UpdateEmailAsync_FailureUpdatingUser_ReturnsFailed()
        {
            var model = new EditEmailViewModel { Email = "newemail@example.com" };
            _userManagerMock.Setup(um => um.ChangeEmailAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(IdentityResult.Failed());
            var result = await _service.UpdateEmailAsync("user-1", model, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Failed));
        }

        [Test]
        public async Task UpdateEmailAsync_Success_ReturnsSuccess()
        {
            var model = new EditEmailViewModel { Email = "newemail@example.com" };
            var result = await _service.UpdateEmailAsync("user-1", model, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Success));
            _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<IdentityUser>()), Times.Once);
            _signInManagerMock.Verify(sm => sm.RefreshSignInAsync(It.IsAny<IdentityUser>()), Times.Once);
        }

        [Test]
        public async Task BuildEditFullNameViewModelAsync_Unauthorized_ReturnsNull()
        {
            var result = await _service.BuildEditFullNameViewModelAsync("user-1", "user-2");
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildEditFullNameViewModelAsync_NotFound_ReturnsNull()
        {
            var result = await _service.BuildEditFullNameViewModelAsync("user-99", "user-99");
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildEditFullNameViewModelAsync_ReturnsFullName()
        {
            var result = await _service.BuildEditFullNameViewModelAsync("user-1", "user-1");
            Assert.IsNotNull(result);
            Assert.That(result.FullName, Is.EqualTo("User One"));
        }

        [Test]
        public async Task UpdateFullNameAsync_Unauthorized_ReturnsUnauthorized()
        {
            var model = new EditFullNameViewModel { FullName = "New Name" };
            var result = await _service.UpdateFullNameAsync("user-1", model, "user-2");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Unauthorized));
        }

        [Test]
        public async Task UpdateFullNameAsync_NotFound_ReturnsNotFound()
        {
            var model = new EditFullNameViewModel { FullName = "New Name" };
            var result = await _service.UpdateFullNameAsync("user-99", model, "user-99");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NotFound));
        }

        [Test]
        public async Task UpdateFullNameAsync_NoChange_ReturnsNoChange()
        {
            var model = new EditFullNameViewModel { FullName = "User One" };
            var result = await _service.UpdateFullNameAsync("user-1", model, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NoChange));
        }

        [Test]
        public async Task UpdateFullNameAsync_Success_ReturnsSuccess()
        {
            var model = new EditFullNameViewModel { FullName = "New Name" };
            var result = await _service.UpdateFullNameAsync("user-1", model, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Success));
            var profile = await _context.UserProfiles.FindAsync("user-1");
            Assert.That(profile.FullName, Is.EqualTo("New Name"));
        }

        [Test]
        public async Task BuildEditPhoneNumberViewModelAsync_Unauthorized_ReturnsNull()
        {
            var result = await _service.BuildEditPhoneNumberViewModelAsync("user-1", "user-2");
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildEditPhoneNumberViewModelAsync_NotFound_ReturnsNull()
        {
            var result = await _service.BuildEditPhoneNumberViewModelAsync("user-99", "user-99");
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildEditPhoneNumberViewModelAsync_ReturnsPhoneNumber()
        {
            var result = await _service.BuildEditPhoneNumberViewModelAsync("user-1", "user-1");
            Assert.IsNotNull(result);
            Assert.That(result.PhoneNumber, Is.EqualTo("1234567890"));
        }

        [Test]
        public async Task UpdatePhoneNumberAsync_Unauthorized_ReturnsUnauthorized()
        {
            var model = new EditPhoneNumberViewModel { PhoneNumber = "5555555" };
            var result = await _service.UpdatePhoneNumberAsync("user-1", model, "user-2");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Unauthorized));
        }

        [Test]
        public async Task UpdatePhoneNumberAsync_NotFound_ReturnsNotFound()
        {
            var model = new EditPhoneNumberViewModel { PhoneNumber = "5555555" };
            var result = await _service.UpdatePhoneNumberAsync("user-99", model, "user-99");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NotFound));
        }

        [Test]
        public async Task UpdatePhoneNumberAsync_NoChange_ReturnsNoChange()
        {
            var model = new EditPhoneNumberViewModel { PhoneNumber = "1234567890" };
            var result = await _service.UpdatePhoneNumberAsync("user-1", model, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NoChange));
        }

        [Test]
        public async Task UpdatePhoneNumberAsync_Success_ReturnsSuccess()
        {
            var model = new EditPhoneNumberViewModel { PhoneNumber = "5555555" };
            var result = await _service.UpdatePhoneNumberAsync("user-1", model, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Success));

            var profile = await _context.UserProfiles.FindAsync("user-1");
            Assert.That(profile.PhoneNumber, Is.EqualTo("5555555"));
        }

        [Test]
        public async Task BuildEditAddressViewModelAsync_Unauthorized_ReturnsNull()
        {
            var result = await _service.BuildEditAddressViewModelAsync("user-1", "user-2");
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildEditAddressViewModelAsync_NotFound_ReturnsNull()
        {
            var result = await _service.BuildEditAddressViewModelAsync("user-99", "user-99");
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildEditAddressViewModelAsync_ReturnsAddress()
        {
            var result = await _service.BuildEditAddressViewModelAsync("user-1", "user-1");
            Assert.IsNotNull(result);
            Assert.That(result.Address, Is.EqualTo("123 Main St"));
        }

        [Test]
        public async Task UpdateAddressAsync_Unauthorized_ReturnsUnauthorized()
        {
            var model = new EditAddressViewModel { Address = "New Address" };
            var result = await _service.UpdateAddressAsync("user-1", model, "user-2");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Unauthorized));
        }

        [Test]
        public async Task UpdateAddressAsync_NotFound_ReturnsNotFound()
        {
            var model = new EditAddressViewModel { Address = "New Address" };
            var result = await _service.UpdateAddressAsync("user-99", model, "user-99");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NotFound));
        }

        [Test]
        public async Task UpdateAddressAsync_NoChange_ReturnsNoChange()
        {
            var model = new EditAddressViewModel { Address = "123 Main St" };
            var result = await _service.UpdateAddressAsync("user-1", model, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NoChange));
        }

        [Test]
        public async Task UpdateAddressAsync_Success_ReturnsSuccess()
        {
            var model = new EditAddressViewModel { Address = "New Address" };
            var result = await _service.UpdateAddressAsync("user-1", model, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Success));

            var profile = await _context.UserProfiles.FindAsync("user-1");
            Assert.That(profile.Address, Is.EqualTo("New Address"));
        }

        [Test]
        public async Task DeleteUserProfileAsync_Unauthorized_ReturnsUnauthorized()
        {
            var result = await _service.DeleteUserProfileAsync("user-1", "user-2");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Unauthorized));
        }

        [Test]
        public async Task DeleteUserProfileAsync_ProfileOrUserNotFound_ReturnsNotFound()
        {
            _userManagerMock.Setup(um => um.FindByIdAsync("user-99")).ReturnsAsync((IdentityUser)null);

            var result = await _service.DeleteUserProfileAsync("user-99", "user-99");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NotFound));
        }

        [Test]
        public async Task DeleteUserProfileAsync_FailureDeletingIdentityUser_ReturnsFailed()
        {
            _userManagerMock.Setup(um => um.DeleteAsync(It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Failed());

            var result = await _service.DeleteUserProfileAsync("user-1", "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Failed));
        }

        [Test]
        public async Task DeleteUserProfileAsync_Success_DeletesProfileAndSignsOut()
        {
            var animalBeforeDelete = await _context.Animals.FirstOrDefaultAsync(a => a.UserProfileId == "user-1");
            Assert.IsNotNull(animalBeforeDelete);

            var animalId = animalBeforeDelete!.Id;

            var result = await _service.DeleteUserProfileAsync("user-1", "user-1");

            Assert.That(result, Is.EqualTo(ServiceOperationResult.Success));

            var profile = await _context.UserProfiles.FindAsync("user-1");
            Assert.IsNull(profile);

            _userManagerMock.Verify(um => um.DeleteAsync(It.IsAny<IdentityUser>()), Times.Once);
            _signInManagerMock.Verify(sm => sm.SignOutAsync(), Times.Once);

            var animal = await _context.Animals.IgnoreQueryFilters().Include(a => a.Appointments).FirstOrDefaultAsync(a => a.Id == animalId);

            Assert.IsNotNull(animal, "Animal should exist even if soft deleted");
            Assert.That(animal!.IsDeleted, Is.True);

            Assert.That(animal.Appointments, Is.Not.Null.And.Not.Empty);
            Assert.That(animal.Appointments.All(appt => appt.IsDeleted), Is.True);

        }


    }
}

