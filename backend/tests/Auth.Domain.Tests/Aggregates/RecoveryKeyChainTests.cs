using Auth.Domain.Aggregates;
using Auth.Domain.Constants;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;
using FluentAssertions;
using SharedKernel;

namespace Auth.Domain.Tests.Aggregates;

public sealed class RecoveryKeyChainTests
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
    private static readonly UserId ValidUserId = UserId.New();

    private static List<RecoverKeyInput> CreateValidRecoveryKeyInputs()
    {
        return Enumerable.Range(1, RecoveryKeyConstants.MaxKeysPerChain)
            .Select(i => RecoverKeyInput.Create($"identifier-{i}", $"verifier-hash-{i}"))
            .ToList();
    }

    private static List<(string identifier, string verifierHash)> CreateValidRecoveryKeyPairs()
    {
        return Enumerable.Range(1, RecoveryKeyConstants.MaxKeysPerChain)
            .Select(i => ($"new-identifier-{i}", $"new-verifier-hash-{i}"))
            .ToList();
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        List<RecoverKeyInput> inputs = CreateValidRecoveryKeyInputs();

        Outcome<RecoveryKeyChain> outcome = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: inputs,
            utcNow: UtcNow
        );

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.UserId.Should().Be(ValidUserId);
        outcome.Value.CreatedAt.Should().Be(UtcNow);
        outcome.Value.LastRotatedAt.Should().BeNull();
        outcome.Value.Version.Should().Be(1);
        outcome.Value.RecoveryKeys.Should().HaveCount(RecoveryKeyConstants.MaxKeysPerChain);
    }

    [Fact]
    public void Create_WithValidData_ShouldGenerateNewId()
    {
        List<RecoverKeyInput> inputs = CreateValidRecoveryKeyInputs();

        Outcome<RecoveryKeyChain> outcome = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: inputs,
            utcNow: UtcNow
        );

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        UserId emptyUserId = UserId.UnsafeFromGuid(Guid.Empty);
        List<RecoverKeyInput> inputs = CreateValidRecoveryKeyInputs();

        Outcome<RecoveryKeyChain> outcome = RecoveryKeyChain.Create
        (
            userId: emptyUserId,
            recoverKeyInputs: inputs,
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(RecoveryKeyChainFaults.UserIdRequiredForCreation);
    }

    [Fact]
    public void Create_WithTooFewKeys_ShouldReturnFailure()
    {
        List<RecoverKeyInput> inputs = Enumerable.Range(1, RecoveryKeyConstants.MaxKeysPerChain - 1)
            .Select(i => RecoverKeyInput.Create($"identifier-{i}", $"verifier-hash-{i}"))
            .ToList();

        Outcome<RecoveryKeyChain> outcome = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: inputs,
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(RecoveryKeyChainFaults.InvalidRecoveryKeyCount);
    }

    [Fact]
    public void Create_WithTooManyKeys_ShouldReturnFailure()
    {
        List<RecoverKeyInput> inputs = Enumerable.Range(1, RecoveryKeyConstants.MaxKeysPerChain + 1)
            .Select(i => RecoverKeyInput.Create($"identifier-{i}", $"verifier-hash-{i}"))
            .ToList();

        Outcome<RecoveryKeyChain> outcome = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: inputs,
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(RecoveryKeyChainFaults.InvalidRecoveryKeyCount);
    }

    [Fact]
    public void Create_WithEmptyKeys_ShouldReturnFailure()
    {
        List<RecoverKeyInput> inputs = [];

        Outcome<RecoveryKeyChain> outcome = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: inputs,
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(RecoveryKeyChainFaults.InvalidRecoveryKeyCount);
    }

    [Fact]
    public void Rotate_WithValidData_ShouldReplaceKeys()
    {
        RecoveryKeyChain chain = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: CreateValidRecoveryKeyInputs(),
            utcNow: UtcNow
        ).Value;

        List<(string identifier, string verifierHash)> newPairs = CreateValidRecoveryKeyPairs();
        DateTimeOffset rotateTime = UtcNow.AddDays(1);

        Outcome outcome = chain.Rotate(newPairs, rotateTime);

        outcome.IsSuccess.Should().BeTrue();
        chain.RecoveryKeys.Should().HaveCount(RecoveryKeyConstants.MaxKeysPerChain);
        chain.LastRotatedAt.Should().Be(rotateTime);
        chain.Version.Should().Be(2);
    }

    [Fact]
    public void Rotate_WithValidData_ShouldUpdateRecoveryKeys()
    {
        RecoveryKeyChain chain = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: CreateValidRecoveryKeyInputs(),
            utcNow: UtcNow
        ).Value;

        List<(string identifier, string verifierHash)> newPairs = CreateValidRecoveryKeyPairs();

        chain.Rotate(newPairs, UtcNow.AddDays(1));

        chain.RecoveryKeys.All(k => k.Identifier.StartsWith("new-", StringComparison.Ordinal)).Should().BeTrue();
    }

    [Fact]
    public void Rotate_WithTooFewKeys_ShouldReturnFailure()
    {
        RecoveryKeyChain chain = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: CreateValidRecoveryKeyInputs(),
            utcNow: UtcNow
        ).Value;

        List<(string identifier, string verifierHash)> newPairs = Enumerable.Range(1, RecoveryKeyConstants.MaxKeysPerChain - 1)
            .Select(i => ($"new-identifier-{i}", $"new-verifier-hash-{i}"))
            .ToList();

        Outcome outcome = chain.Rotate(newPairs, UtcNow.AddDays(1));

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(RecoveryKeyChainFaults.InvalidRecoveryKeyCount);
    }

    [Fact]
    public void Rotate_WithTooManyKeys_ShouldReturnFailure()
    {
        RecoveryKeyChain chain = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: CreateValidRecoveryKeyInputs(),
            utcNow: UtcNow
        ).Value;

        List<(string identifier, string verifierHash)> newPairs = Enumerable.Range(1, RecoveryKeyConstants.MaxKeysPerChain + 1)
            .Select(i => ($"new-identifier-{i}", $"new-verifier-hash-{i}"))
            .ToList();

        Outcome outcome = chain.Rotate(newPairs, UtcNow.AddDays(1));

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(RecoveryKeyChainFaults.InvalidRecoveryKeyCount);
    }

    [Fact]
    public void Rotate_WithEmptyKeys_ShouldReturnFailure()
    {
        RecoveryKeyChain chain = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: CreateValidRecoveryKeyInputs(),
            utcNow: UtcNow
        ).Value;

        List<(string identifier, string verifierHash)> newPairs = [];

        Outcome outcome = chain.Rotate(newPairs, UtcNow.AddDays(1));

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(RecoveryKeyChainFaults.InvalidRecoveryKeyCount);
    }

    [Fact]
    public void Rotate_MultipleTimes_ShouldIncrementVersion()
    {
        RecoveryKeyChain chain = RecoveryKeyChain.Create
        (
            userId: ValidUserId,
            recoverKeyInputs: CreateValidRecoveryKeyInputs(),
            utcNow: UtcNow
        ).Value;

        chain.Rotate(CreateValidRecoveryKeyPairs(), UtcNow.AddDays(1));
        chain.Rotate(CreateValidRecoveryKeyPairs(), UtcNow.AddDays(2));
        chain.Rotate(CreateValidRecoveryKeyPairs(), UtcNow.AddDays(3));

        chain.Version.Should().Be(4);
    }
}

