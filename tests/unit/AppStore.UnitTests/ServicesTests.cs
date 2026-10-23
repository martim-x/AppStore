using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using AppStore.Core.Data;
using Project.Services;
using Xunit;
using FluentAssertions;

namespace AppStore.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IRepository> _repoMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly SHA256Hasher _sha256Hasher;
        private readonly AuthService _authService;

        // Real BCrypt hashes (work factor 12)
        private const string HashOldPass = "$2a$12$Nv39VG./Y9NbG8iX.Vbxw.5XjBeZz.mq0gDjBSEb/Y8wW/yLPc4sq"; // "oldpass"
        private const string HashNewPass = "$2a$12$xrHHvSmIWCpCtZIy7s38ueX4Kyfmh69fvQ3Di4a5avr5SAHJnVxja"; // "newpass"
        private const string HashPassword = "$2a$12$V4zaGZews.4dfKfhB8vcj.Cl.H0HNhM6t5JBC4QerBllRiZL454dO"; // "password"
        private const string HashTestPass123 = "$2a$12$AbYyWITYw79Zm.x2/6LDSOgXNrws2h3ZH20DDbtRH4.M3xVSF91Li"; // "testpass123"

        public AuthServiceTests()
        {
            _repoMock = new Mock<IRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _sha256Hasher = new SHA256Hasher();
            _authService = new AuthService(_repoMock.Object, _passwordHasherMock.Object, _sha256Hasher);
        }

        [Fact]
        public void IsLegacySha256Hash_BCryptHash_ReturnsFalse()
        {
            var user = new User { Id = Guid.NewGuid(), Login = "testuser", PasswordHash = HashPassword, Role = UserRole.User };
            _repoMock.Setup(r => r.GetUserByLogin("testuser")).Returns(user);

            _authService.Login("testuser", "password");

            _repoMock.Verify(r => r.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void IsLegacySha256Hash_SHA256Hash_ReturnsTrue()
        {
            var sha256Hash = AuthService.HashPasswordStatic("password");
            var user = new User { Id = Guid.NewGuid(), Login = "legacyuser", PasswordHash = sha256Hash, Role = UserRole.User };
            _repoMock.Setup(r => r.GetUserByLogin("legacyuser")).Returns(user);
            _passwordHasherMock.Setup(h => h.HashPassword("password")).Returns(HashPassword);

            _authService.Login("legacyuser", "password");

            _repoMock.Verify(r => r.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public void Login_RehashFails_StillAuthenticatesAndRestoresHash()
        {
            // Сбой БД при обновлении хеша не должен мешать аутентификации:
            // пароль верный — значит пользователь входит. Хеш при этом
            // возвращается к прежнему, чтобы несохранённый BCrypt-хеш
            // не остался в DbContext.
            var sha256Hash = AuthService.HashPasswordStatic("password");
            var user = new User { Id = Guid.NewGuid(), Login = "legacyuser", PasswordHash = sha256Hash, Role = UserRole.User };
            _repoMock.Setup(r => r.GetUserByLogin("legacyuser")).Returns(user);
            _passwordHasherMock.Setup(h => h.HashPassword("password")).Returns(HashPassword);
            _repoMock.Setup(r => r.UpdateUser(It.IsAny<User>())).Throws(new InvalidOperationException("DB unavailable"));

            var result = _authService.Login("legacyuser", "password");

            result.Should().NotBeNull();
            result!.Login.Should().Be("legacyuser");
            user.PasswordHash.Should().Be(sha256Hash);
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsUser()
        {
            var user = new User { Id = Guid.NewGuid(), Login = "testuser", PasswordHash = HashTestPass123, Role = UserRole.User };

            _repoMock.Setup(r => r.GetUserByLogin("testuser")).Returns(user);

            var result = _authService.Login("testuser", "testpass123");

            result.Should().NotBeNull();
            result!.Login.Should().Be("testuser");
            _authService.CurrentUser.Should().BeSameAs(user);
            _authService.IsAdmin.Should().BeFalse();
        }

        [Fact]
        public void Login_InvalidPassword_ReturnsNull()
        {
            var user = new User { Id = Guid.NewGuid(), Login = "testuser", PasswordHash = HashTestPass123, Role = UserRole.User };
            _repoMock.Setup(r => r.GetUserByLogin("testuser")).Returns(user);

            var result = _authService.Login("testuser", "wrongpassword");

            result.Should().BeNull();
            _authService.CurrentUser.Should().BeNull();
        }

        [Fact]
        public void Login_NonExistentUser_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetUserByLogin("nonexistent")).Returns((User?)null);

            var result = _authService.Login("nonexistent", "anypassword");

            result.Should().BeNull();
        }

        [Fact]
        public void Login_CaseInsensitive_Works()
        {
            var user = new User { Id = Guid.NewGuid(), Login = "TestUser", PasswordHash = HashTestPass123, Role = UserRole.User };
            _repoMock.Setup(r => r.GetUserByLogin(It.Is<string>(s => s.Equals("testuser", StringComparison.OrdinalIgnoreCase)))).Returns(user);

            var result = _authService.Login("TESTUSER", "testpass123");

            result.Should().NotBeNull();
        }

        [Fact]
        public void Login_LegacySha256_AutoRehashesToBCrypt()
        {
            var password = "password";
            var sha256Hash = AuthService.HashPasswordStatic(password); // legacy SHA-256
            var bcryptHash = HashPassword; // BCrypt hash of "password"
            var user = new User { Id = Guid.NewGuid(), Login = "legacyuser", PasswordHash = sha256Hash, Role = UserRole.User };

            _repoMock.Setup(r => r.GetUserByLogin("legacyuser")).Returns(user);
            _passwordHasherMock.Setup(h => h.HashPassword(password)).Returns(bcryptHash);
            _repoMock.Setup(r => r.UpdateUser(user)).Callback(() => user.PasswordHash = bcryptHash);

            var result = _authService.Login("legacyuser", password);

            result.Should().NotBeNull();
            _repoMock.Verify(r => r.UpdateUser(user), Times.Once);
            user.PasswordHash.Should().Be(bcryptHash); // rehashed to BCrypt
        }

        [Fact]
        public void Logout_ClearsCurrentUser()
        {
            var user = new User { Id = Guid.NewGuid(), Login = "testuser", PasswordHash = HashTestPass123, Role = UserRole.User };
            _repoMock.Setup(r => r.GetUserByLogin("testuser")).Returns(user);
            _authService.Login("testuser", "testpass123");

            _authService.Logout();

            _authService.CurrentUser.Should().BeNull();
            _authService.IsAdmin.Should().BeFalse();
        }

        [Fact]
        public void IsAdmin_AdminUser_ReturnsTrue()
        {
            var user = new User { Id = Guid.NewGuid(), Login = "admin", PasswordHash = HashPassword, Role = UserRole.Admin };
            _repoMock.Setup(r => r.GetUserByLogin("admin")).Returns(user);
            _authService.Login("admin", "password");

            _authService.IsAdmin.Should().BeTrue();
        }

        [Fact]
        public void IsAdmin_RegularUser_ReturnsFalse()
        {
            var user = new User { Id = Guid.NewGuid(), Login = "user", PasswordHash = HashPassword, Role = UserRole.User };
            _repoMock.Setup(r => r.GetUserByLogin("user")).Returns(user);
            _authService.Login("user", "password");

            _authService.IsAdmin.Should().BeFalse();
        }

        [Fact]
        public void ChangePassword_CorrectOldPassword_UpdatesHash()
        {
            var user = new User { Id = Guid.NewGuid(), Login = "testuser", PasswordHash = HashOldPass, Role = UserRole.User };
            _repoMock.Setup(r => r.GetUserByLogin("testuser")).Returns(user);
            _passwordHasherMock.Setup(h => h.HashPassword("newpass")).Returns(HashNewPass);

            _authService.Login("testuser", "oldpass");

            _authService.ChangePassword("oldpass", "newpass");

            user.PasswordHash.Should().Be(HashNewPass);
            _repoMock.Verify(r => r.UpdateUser(user), Times.Once);
        }

        [Fact]
        public void ChangePassword_WrongOldPassword_ThrowsUnauthorizedAccessException()
        {
            var sha256OldPass = AuthService.HashPasswordStatic("oldpass");
            var user = new User { Id = Guid.NewGuid(), Login = "testuser", PasswordHash = sha256OldPass, Role = UserRole.User };
            _repoMock.Setup(r => r.GetUserByLogin("testuser")).Returns(user);
            _passwordHasherMock.Setup(h => h.HashPassword("oldpass")).Returns(HashOldPass);
            _authService.Login("testuser", "oldpass");

            Action act = () => _authService.ChangePassword("wrongpass", "newpass");

            act.Should().Throw<UnauthorizedAccessException>()
               .WithMessage("Old password is incorrect.");
        }

        [Fact]
        public void ChangePassword_NoUserLoggedIn_ThrowsInvalidOperationException()
        {
            Action act = () => _authService.ChangePassword("old", "new");

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("No user is logged in.");
        }

        [Fact]
        public void UpdateProfile_LoggedInUser_UpdatesFields()
        {
            var user = new User { Id = Guid.NewGuid(), Login = "testuser", PasswordHash = HashPassword, Role = UserRole.User };
            _repoMock.Setup(r => r.GetUserByLogin("testuser")).Returns(user);
            _authService.Login("testuser", "password");

            _authService.UpdateProfile("John", "Doe", "john@example.com");

            user.FirstName.Should().Be("John");
            user.LastName.Should().Be("Doe");
            user.Email.Should().Be("john@example.com");
            _repoMock.Verify(r => r.UpdateUser(user), Times.Once);
        }

        [Fact]
        public void HashPassword_SameInput_ProducesSameHash()
        {
            var hash1 = AuthService.HashPasswordStatic("testpass");
            var hash2 = AuthService.HashPasswordStatic("testpass");

            hash1.Should().Be(hash2);
            hash1.Should().HaveLength(64); // SHA-256 hex
        }

        [Fact]
        public void HashPassword_DifferentInput_ProducesDifferentHash()
        {
            var hash1 = AuthService.HashPasswordStatic("pass1");
            var hash2 = AuthService.HashPasswordStatic("pass2");

            hash1.Should().NotBe(hash2);
        }
    }

    public class UndoRedoServiceTests
    {
        private readonly UndoRedoService _service;

        public UndoRedoServiceTests()
        {
            _service = new UndoRedoService();
        }

        [Fact]
        public void InitialState_CanUndoAndCanRedoAreFalse()
        {
            _service.CanUndo.Should().BeFalse();
            _service.CanRedo.Should().BeFalse();
        }

        [Fact]
        public void PushAction_CanUndoBecomesTrue()
        {
            _service.Push(new TestUndoRedoAction());
            _service.CanUndo.Should().BeTrue();
        }

        [Fact]
        public void PushMultipleActions_CanUndoTrue()
        {
            _service.Push(new TestUndoRedoAction());
            _service.Push(new TestUndoRedoAction());
            _service.CanUndo.Should().BeTrue();
        }

        [Fact]
        public void Undo_ExecutesUndoAction()
        {
            var executed = false;
            _service.Push(new TestUndoRedoAction(undo: () => executed = true));

            _service.Undo();

            executed.Should().BeTrue();
        }

        [Fact]
        public void Undo_ThenRedo_ExecutesRedoAction()
        {
            var undoExecuted = false;
            var redoExecuted = false;
            _service.Push(new TestUndoRedoAction(
                undo: () => undoExecuted = true,
                redo: () => redoExecuted = true));

            _service.Undo();
            _service.Redo();

            undoExecuted.Should().BeTrue();
            redoExecuted.Should().BeTrue();
        }

        [Fact]
        public void Undo_WhenEmpty_DoesNothing()
        {
            Action act = () => _service.Undo();
            act.Should().NotThrow();
        }

        [Fact]
        public void Redo_WhenEmpty_DoesNothing()
        {
            Action act = () => _service.Redo();
            act.Should().NotThrow();
        }

        [Fact]
        public void Clear_RemovesAllActions()
        {
            _service.Push(new TestUndoRedoAction());
            _service.Clear();

            _service.CanUndo.Should().BeFalse();
            _service.CanRedo.Should().BeFalse();
        }

        private class TestUndoRedoAction : IUndoRedoAction
        {
            private readonly Action _undo;
            private readonly Action _redo;

            public TestUndoRedoAction(Action? undo = null, Action? redo = null)
            {
                _undo = undo ?? (() => { });
                _redo = redo ?? (() => { });
            }

            public string Name => "TestAction";
            public void Undo() => _undo();
            public void Redo() => _redo();
        }
    }
}
