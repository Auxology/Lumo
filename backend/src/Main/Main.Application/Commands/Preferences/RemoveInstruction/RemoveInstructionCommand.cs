using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Preferences.RemoveInstruction;

public sealed record RemoveInstructionCommand(string InstructionId) : ICommand;